// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;

namespace System.Resources
{
    public static class Utils
    {
        //------------------------------------------------------------------------------
        // <copyright file="ClientUtils.cs" company="Microsoft">
        //     Copyright (c) Microsoft Corporation.  All rights reserved.
        // </copyright>
        //------------------------------------------------------------------------------
        public static bool IsCriticalException(Exception ex)
        {
            return ex is NullReferenceException
                    || ex is StackOverflowException
                    || ex is OutOfMemoryException
                    || ex is System.Threading.ThreadAbortException
                    || ex is IndexOutOfRangeException
                    || ex is AccessViolationException;
        }

        //------------------------------------------------------------------------------
        // <copyright file="MultitargetUtil.cs" company="Microsoft">
        //     Copyright (c) Microsoft Corporation.  All rights reserved.
        // </copyright>                                                                
        //------------------------------------------------------------------------------
        /// <devdoc>
        ///     This method gets assembly info for the corresponding type. If the delegate
        ///     is provided it is used to get this information.
        /// </devdoc>
        public static string GetAssemblyQualifiedName(Type type, Func<Type, string> typeNameConverter)
        {
            string assemblyQualifiedName = null;

            if (type != null)
            {
                if (typeNameConverter != null)
                {
                    try
                    {
                        assemblyQualifiedName = typeNameConverter(type);
                    }
                    catch (Exception e)
                    {
                        if (IsSecurityOrCriticalException(e))
                        {
                            throw;
                        }
                    }
                }

                if (string.IsNullOrEmpty(assemblyQualifiedName))
                {
                    assemblyQualifiedName = type.AssemblyQualifiedName;
                }
            }

            return assemblyQualifiedName;
        }
        private static bool IsSecurityOrCriticalException(Exception ex)
        {
            return ex is NullReferenceException
                    || ex is StackOverflowException
                    || ex is OutOfMemoryException
                    || ex is System.Threading.ThreadAbortException
                    || ex is IndexOutOfRangeException
                    || ex is AccessViolationException
                    || ex is System.Security.SecurityException;
        }
    }
}
