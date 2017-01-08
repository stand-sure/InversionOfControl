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

  public class Container
  {
    private readonly IDictionary<Type, Type> dependcies;

    public Container()
    {
      this.dependcies = new Dictionary<Type, Type>();
    }

    public void Register<TService, TServiceProvider>() 
      where TServiceProvider : TService
      where TService : class
    {
      this.dependcies.Add(typeof(TService), typeof(TServiceProvider));
    }

    public T Resolve<T>() where T : class
    {
      var retval = this.Resolve(typeof(T)) as T;
      if (retval == null)
      {
        retval = default(T);
      }

      return retval;
    }

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

    private Type ResolveServiceProvider(Type type)
    {
      return this.dependcies
                 .Where(kvp => kvp.Key == type)
                 .Select(kvp => kvp.Value)    
                 .FirstOrDefault();
    }

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

    private IEnumerable<object> ResolveParameters(IEnumerable<ParameterInfo> parameters)
    {
      return parameters.Select(param => this.Resolve(param.ParameterType))
                       .ToList();
    }
  }
}
