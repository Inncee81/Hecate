// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if net45
using System;
using System.Collections.Generic;

namespace SE.Hecate.Sharp
{
    //------------------------------------------------------------------------------
    // <auto-generated>
    //     This code was generated by a tool.
    //     Runtime Version:4.0.30319.42000
    //
    //     Changes to this file may cause incorrect behavior and will be lost if
    //     the code is regenerated.
    // </auto-generated>
    //------------------------------------------------------------------------------
    public partial class AssemblyCache
    {
        const string ReferenceAssemblyPath = "%ProgramFiles%/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.5";
        const string ReferenceAssembliesAlternativePath = "%ProgramFiles(x86)%/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.5";

        private static void PopulateFrameworkAssemblyList(HashSet<string> frameworkAssemblies)
        {
            frameworkAssemblies.Add("Accessibility, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("CustomMarshalers, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("ISymWrapper, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.Activities.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.Build.Conversion.v4.0, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.Build.Engine, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.Build.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.Build.Tasks.v4.0, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.Build.Utilities.v4.0, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.JScript, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.VisualBasic.Compatibility, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.VisualBasic.Compatibility.Data, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.VisualC, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("Microsoft.VisualC.STLCLR, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("PresentationFramework.Aero2, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("PresentationFramework.AeroLite, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("ReachFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Activities.Core.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Activities.DurableInstancing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Activities.Statements, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.AddIn.Contract, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.AddIn, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.ComponentModel.Composition, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.ComponentModel.Composition.Registration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Configuration.Install, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data.DataSetExtensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data.Entity.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data.OracleClient, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data.Services.Client, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data.Services.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data.Services, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Data.SqlXml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Deployment, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Device, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.DirectoryServices.AccountManagement, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.DirectoryServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.DirectoryServices.Protocols, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Dynamic, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.IdentityModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.IdentityModel.Selectors, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.IdentityModel.Services, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.IO.Log, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.IO.Compression, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Management, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Management.Instrumentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Messaging, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Net, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Net.Http, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Net.Http.WebRequest, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Printing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Reflection.Context, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Runtime.DurableInstancing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Runtime.Serialization.Formatters.Soap, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ServiceModel.Activation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.ServiceModel.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.ServiceModel.Channels, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.ServiceModel.Discovery, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.ServiceModel.Routing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.ServiceModel.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.ServiceProcess, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Speech, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Transactions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Web.Abstractions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Web.ApplicationServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Web.DataVisualization.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Web.DynamicData.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Web.DynamicData, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Web.Entity.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Web.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Web.Extensions.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Web.Mobile, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Web.RegularExpressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Web.Routing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Web.Services, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Windows, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Windows.Forms.DataVisualization.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Windows.Forms.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Windows.Input.Manipulations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Windows.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Workflow.ComponentModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.WorkflowServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("System.Xml.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            frameworkAssemblies.Add("UIAutomationClient, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("UIAutomationClientsideProviders, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("UIAutomationProvider, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("UIAutomationTypes, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("XamlBuildTask, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            frameworkAssemblies.Add("System.Collections, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Collections.Concurrent, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ComponentModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ComponentModel.Annotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ComponentModel.EventBasedAsync, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Diagnostics.Contracts, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Diagnostics.Debug, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Diagnostics.Tools, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Diagnostics.Tracing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Dynamic.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Globalization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.IO, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Linq.Expressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Linq.Parallel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Linq.Queryable, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Net.NetworkInformation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Net.Primitives, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Net.Requests, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ObjectModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Reflection, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Reflection.Emit, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Reflection.Emit.ILGeneration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Reflection.Emit.Lightweight, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Reflection.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Reflection.Primitives, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Resources.ResourceManager, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Runtime.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Runtime.InteropServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Runtime.InteropServices.WindowsRuntime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Runtime.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Runtime.Serialization.Json, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Runtime.Serialization.Primitives, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Runtime.Serialization.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Security.Principal, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ServiceModel.Duplex, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ServiceModel.Http, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ServiceModel.NetTcp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ServiceModel.Primitives, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.ServiceModel.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Text.Encoding, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Text.Encoding.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Text.RegularExpressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Threading, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Threading.Tasks, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Threading.Tasks.Parallel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Xml.ReaderWriter, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Xml.XDocument, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            frameworkAssemblies.Add("System.Xml.XmlSerializer, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        }
    }
}
#endif
