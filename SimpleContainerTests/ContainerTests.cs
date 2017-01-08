// -----------------------------------------------------------------------
//  <copyright file="ContainerTests.cs" company="Stand Sure LLC">
//      Copyright (c) 2017 Stand Sure LLC. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;

namespace SimpleContainerTests
{
  using NUnit.Framework;
  using System;
  using Container = SimpleContainer.Container;

  [TestFixture()]
  public class ContainerTests : AssertionHelper
  {
    Container target;

    [SetUp]
    public void SetUp()
    {
      this.target = new Container();
    }

    [Test]
    public void Container_Should_Have_Register_Method()
    {
      var actual = target.GetType().GetMethod("Register");

      this.Expect(actual, Is.Not.Null);
    }

    [Test]
    public void Container_Should_Have_Resolve_Method()
    {
      var actual = target.GetType().GetMethod("Resolve");

      this.Expect(actual, Is.Not.Null);
    }

    [Test]
    public void Registered_ServiceProvider_Should_Resolve()
    {
      target.Register<IList<string>, List<string>>();

      var actual = target.Resolve<IList<string>>();

      this.Expect(actual, Is.Not.Null);
      this.Expect(actual, Is.InstanceOf<IList<string>>());
      this.Expect(actual, Is.InstanceOf<List<string>>());
    }

    private interface IFoo
    {
      void Execute();
    }

    private class Foo : IFoo
    {
      public Foo(string text)
      {
      }

      public void Execute()
      {
      }
    }

    [Test]
    public void Container_Should_Be_Able_To_Handle_ctor_with_Parameters()
    {
      target.Register<IFoo, Foo>();

      var actual = target.Resolve<IFoo>();

      this.Expect(actual, Is.Not.Null);
      this.Expect(actual, Is.InstanceOf<IFoo>());
      this.Expect(actual, Is.InstanceOf<Foo>());
    }

    private interface IBar
    {
    }

    private class Bar : IBar
    {
      public Bar(IFoo foo)
      {
      }
    }

    [Test]
    public void Container_Should_Be_Able_To_Handle_Nested_Known_Types()
    {
      target.Register<IFoo, Foo>();
      target.Register<IBar, Bar>();

      var actual = target.Resolve<IBar>();
      this.Expect(actual, Is.Not.Null);
      this.Expect(actual, Is.InstanceOf<IBar>());
      this.Expect(actual, Is.InstanceOf<Bar>());
    }

  }
}
