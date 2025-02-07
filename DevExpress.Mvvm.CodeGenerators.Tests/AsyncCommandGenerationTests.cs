﻿using NUnit.Framework;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [GenerateViewModel]
    partial class GenerateAsyncCommands {
        readonly Task<int> task = new(() => 1);
        [GenerateCommand]
        public Task WithNoArg() => Task.CompletedTask;
        [GenerateCommand]
        public Task WithArg(int arg) => Task.CompletedTask;
        [GenerateCommand]
        public Task WithNullableArg(int? arg) => Task.CompletedTask;
        public Task SomeMethod() => Task.CompletedTask;

        [GenerateCommand(Name = "MyAsyncCommand", CanExecuteMethod = "CanDoIt")]
        public Task Method(int arg) => Task.CompletedTask;
        public bool CanDoIt(int arg) => arg > 0;

        [GenerateCommand(Name = "AsyncCommandWithoutCommandManager", UseCommandManager = false)]
        public Task WithoutManager() => Task.CompletedTask;

        [GenerateCommand(AllowMultipleExecution = true)]
        public Task AllowMultipleExecution() => Task.CompletedTask;

        [GenerateCommand]
        public Task<int> GenericTask() => task;
    }

    [TestFixture]
    public class AsyncCommandGenerationTests {
        [Test]
        public void AsyncCommandImplementation() {
            var generated = new GenerateAsyncCommands();

            Assert.IsNotNull(generated.GetType().GetProperty("WithNoArgCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithArgCommand"));
            Assert.IsNotNull(generated.GetType().GetProperty("WithNullableArgCommand"));

            Assert.IsNull(generated.GetType().GetProperty("With2ArgsCommand"));
            Assert.IsNull(generated.GetType().GetProperty("ReturnNoTaskCommand"));
            Assert.IsNull(generated.GetType().GetProperty("SomeMethodCommand"));
        }

        [Test]
        public void CallRequiredMethodForAsyncCommand() {
            var generated = new GenerateAsyncCommands();

            var method = GetFieldValue<Func<int, Task>, AsyncCommand<int>>(generated.MyAsyncCommand, "executeMethod");
            var expectedMethod = generated.GetType().GetMethod("Method");
            Assert.AreEqual(expectedMethod, method.Method);

            var canMethod = GetFieldValue<Func<int, bool>, AsyncCommand<int>>(generated.MyAsyncCommand, "canExecuteMethod");
            var expectedCanMethod = generated.GetType().GetMethod("CanDoIt");
            Assert.AreEqual(expectedCanMethod, canMethod.Method);

            var useCommandManager = GetFieldValue<bool, AsyncCommand<int>>(generated.MyAsyncCommand, "useCommandManager");
            var expectedUseCommandManager = true;
            Assert.AreEqual(expectedUseCommandManager, useCommandManager);

            var allowMultipleExecution = generated.MyAsyncCommand.AllowMultipleExecution;
            var expectedAllowMultipleExecution = false;
            Assert.AreEqual(expectedAllowMultipleExecution, allowMultipleExecution);

            useCommandManager = GetFieldValue<bool, AsyncCommand>(generated.AsyncCommandWithoutCommandManager, "useCommandManager");
            expectedUseCommandManager = false;
            Assert.AreEqual(expectedUseCommandManager, useCommandManager);

            allowMultipleExecution = generated.AllowMultipleExecutionCommand.AllowMultipleExecution;
            expectedAllowMultipleExecution = true;
            Assert.AreEqual(expectedAllowMultipleExecution, allowMultipleExecution);

            var canExecuteMethod = GetFieldValue<Func<int, bool>, AsyncCommand>(generated.GenericTaskCommand, "canExecuteMethod");
            Assert.IsNull(canExecuteMethod);
        }
        static TResult GetFieldValue<TResult, T>(T source, string fieldName) {
            var fieldInfo = source.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo);

            return (TResult)fieldInfo.GetValue(source);
        }

        [Test]
        public void ArgumentTypeForAsyncCommand() {
            var generated = new GenerateAsyncCommands();

            var noArgumentType = generated.WithNoArgCommand.GetType();
            Assert.IsEmpty(noArgumentType.GetGenericArguments());
            var expectedType = typeof(AsyncCommand);
            Assert.AreEqual(expectedType, noArgumentType);

            var intArgumentType = generated.WithArgCommand.GetType().GetGenericArguments()[0];
            var intExpectedType = typeof(int);
            Assert.AreEqual(intExpectedType, intArgumentType);

            var nullableIntArgumentType = generated.WithNullableArgCommand.GetType().GetGenericArguments()[0];
            var nullableIntExpectedType = typeof(int?);
            Assert.AreEqual(nullableIntExpectedType, nullableIntArgumentType);
        }
    }
}
