// -----------------------------------------------------------------------
//  <copyright file="Container.cs" company="Stand Sure LLC">
//      Copyright (c) 2017 Stand Sure LLC. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace SimpleContainer
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  /// <summary>
  /// A simple 
  /// </summary>
  public class Container
  {
    /// <summary>
    /// The dependcies.
    /// </summary>
    private readonly IDictionary<Type, Type> dependencies;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:SimpleContainer.Container"/> class.
    /// </summary>
    public Container()
    {
      this.dependencies = new Dictionary<Type, Type>();
    }

    /// <summary>
    /// Registers TServiceProvider as a provider for TService.
    /// </summary>
    /// <typeparam name="TService">The service interface type.</typeparam>
    /// <typeparam name="TServiceProvider">The service provider type.</typeparam>
    public void Register<TService, TServiceProvider>()
      where TServiceProvider : TService
      where TService : class
    {
      if (!typeof(TService).IsInterface)
      {
        throw new ArgumentException("TService must be an interface");
      }

      if (!typeof(TServiceProvider).GetConstructors().Any())
      {
        throw new ArgumentException("TServiceProvider must have a public constructor");
      }

      this.dependencies.Add(typeof(TService), typeof(TServiceProvider));
    }

    /// <summary>
    /// Resolves the service provider from the service interface key.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>An instance of the service provider.</returns>
    public T Resolve<T>() where T : class
    {
      var retval = this.Resolve(typeof(T)) as T;
      if (retval == null)
      {
        retval = default(T);
      }

      return retval;
    }

    /// <summary>
    /// Resolves the specified type.
    /// </summary>
    /// <param name="type">The service type.</param>
    /// <returns>An instance of the service provider type.</returns>
    private object Resolve(Type type)
    {
      object retval = null;

      Type providerType = this.ResolveServiceProvider(type);
      if (providerType != null)
      {
        retval = this.BuildKnownType(providerType);
      }
      else
      {
        retval = this.BuildUnknownType(type);
      }

      return retval;
    }

    /// <summary>
    /// Resolves the service provider.
    /// </summary>
    /// <returns>The service provider type.</returns>
    /// <param name="type">The service type.</param>
    /// <remarks>May return null.</remarks>
    private Type ResolveServiceProvider(Type type)
    {
      return this.dependencies
                 .Where(kvp => kvp.Key == type)
                 .Select(kvp => kvp.Value)
                 .FirstOrDefault();
    }

    /// <summary>
    /// Builds a known/registered type.
    /// </summary>
    /// <returns>An instance of the registered type.</returns>
    /// <param name="providerType">The provider type.</param>
    /// <remarks>May throw an exception.</remarks>
    private object BuildKnownType(Type providerType)
    {
      object retval = null;

      var constructor = providerType.GetConstructors()
                                    .FirstOrDefault();
      if (constructor != null)
      {
        var parameters = constructor.GetParameters();
        if (!parameters.Any())
        {
          retval = Activator.CreateInstance(providerType);
        }
        else
        {
          retval = constructor.Invoke(
            this.ResolveParameters(parameters)
                .ToArray()
          );
        }
      }

      return retval;
    }

    /// <summary>
    /// Attempts to build unknown/unregistered type.
    /// </summary>
    /// <returns>The unknown type or null.</returns>
    /// <param name="type">The Type.</param>
    /// <remarks>Will not throw.</remarks>
    private object BuildUnknownType(Type type)
    {
      object retval;
      try
      {
        retval = Activator.CreateInstance(type);
      }
      catch
      {
        retval = null;
      }

      return retval;
    }

    /// <summary>
    /// Resolves the parameters.
    /// </summary>
    /// <returns>The an <see cref="System.Collections.Generic.IEnumerable{Object}"/> containing instances of the parameters.</returns>
    /// <param name="parameters">An enumerable of <see cref="System.Reflection.ParameterInfo"/></param>
    private IEnumerable<object> ResolveParameters(IEnumerable<ParameterInfo> parameters)
    {
      return parameters.Select(param => this.Resolve(param.ParameterType))
                       .ToList();
    }
  }
}