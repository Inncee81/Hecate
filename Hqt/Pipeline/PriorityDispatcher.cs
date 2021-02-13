// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime;
using System.Threading;
using SE.Actor;
using SE.Reactive;

namespace SE.Hecate
{
    /// <summary>
    /// A priority list based endpoint dispatching strategy
    /// </summary>
    public class PriorityDispatcher : FinalizerObject, IDispatcher<KernelMessage>
    {
        List<IPrioritizedActor> subscriptions;
        ReadWriteLock subscriptionLock;

        public int Count
        {
            get { return subscriptions.Count; }
        }

        /// <summary>
        /// Creates a new dispatcher instance
        /// </summary>
        public PriorityDispatcher()
        {
            this.subscriptions = new List<IPrioritizedActor>();
            this.subscriptionLock = new ReadWriteLock();
        }
        protected override void Dispose(bool disposing)
        {
            Clear();
            base.Dispose(disposing);
        }

        public void Register(IReceiver<KernelMessage, bool> observer)
        {
            IPrioritizedActor actor = observer as IPrioritizedActor;
            if (actor == null)
            {
                throw new ArgumentException();
            }
            subscriptionLock.WriteLock();
            try
            {
                subscriptions.Add(actor);

                Sort();

                actor.Attach(this);
            }
            finally
            {
                subscriptionLock.WriteRelease();
            }
        }
        public void Remove(IReceiver<KernelMessage, bool> observer)
        {
            subscriptionLock.WriteLock();
            try
            { 
                List<IPrioritizedActor>.Enumerator iterator = subscriptions.GetEnumerator();
                for (int i = 0; iterator.MoveNext(); i++)
                {
                    if (iterator.Current == observer)
                    {
                        subscriptions.RemoveAt(i);

                        Sort();

                        iterator.Current.Detach(this);
                        iterator.Current.OnCompleted();
                    }
                }
            }
            catch (AppDomainUnloadedException)
            { }
            catch (Exception er)
            {
                try
                {
                    observer.OnError(er);
                }
                catch { }
            }
            finally
            {
                subscriptionLock.WriteRelease();
            }
        }

        public bool Dispatch(ref KernelMessage message)
        {
            bool dispatched = false;
            subscriptionLock.ReadLock();
            try
            {
                IEnumerator<IPrioritizedActor> iterator = subscriptions.GetEnumerator();
                for (int i = -1; iterator.MoveNext() && (i < 0 || i == iterator.Current.Priority);)
                {
                    IPrioritizedActor observer = iterator.Current;
                    try
                    {
                        if (observer.OnNext(message) && !dispatched)
                        {
                            i = iterator.Current.Priority;
                            dispatched = true;
                        }
                    }
                    catch (Exception er)
                    {
                        try
                        {
                            Application.Error(er);
                            observer.OnError(er);
                        }
                        catch { }
                    }
                }
            }
            finally
            {
                subscriptionLock.ReadRelease();
            }
            return dispatched;
        }

        /// <summary>
        /// Sorts the endpoints based on their priority
        /// </summary>
        public void Sort()
        {
            subscriptionLock.WriteLock();
            try
            {
                subscriptions.Sort((c1, c2) =>
                {
                    try
                    {
                        return -(c1.Priority.CompareTo(c2.Priority));
                    }
                    catch (AppDomainUnloadedException)
                    {
                        return 0;
                    }
                });
            }
            finally
            {
                subscriptionLock.WriteRelease();
            }
        }

        public void Clear()
        {
            subscriptionLock.WriteLock();
            try
            {
                foreach (IPrioritizedActor observer in subscriptions)
                {
                    try
                    {
                        observer.OnCompleted();
                        observer.Detach(this);
                    }
                    catch (AppDomainUnloadedException)
                    { }
                    catch (Exception er)
                    {
                        try
                        {
                            observer.OnError(er);
                        }
                        catch { }
                    }
                }
                subscriptions.Clear();
            }
            finally
            {
                subscriptionLock.WriteRelease();
            }
        }
    }
}