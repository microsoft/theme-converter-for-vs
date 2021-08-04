// Copyright (c) Microsoft. All Rights Reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.using System;
using System.Collections;
#pragma warning disable IDE0051 // Remove unused private members
namespace ConsoleTest
{
    using System.Drawing;
    using static Math;
    internal class Colors
    {
        static event EventHandler MyEvent;
        const string myConst = "Hello";
        readonly int myField = 5;
        const int myInput = 4000;
        string MyProperty => "World";
        // A regular comment for good measure.
        public Colors()
        {
             MyMethod(myField, myConst);
        }
        ~Colors() { }
        /// <summary>
        /// Member <see langword="void"/> method of <see cref="Colors"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="myParameter">This is my parameter</param>
        /// <param name="greeting">My greeting</param>
        public void MyMethod<T>(T myParameter, string greeting)
        {
            const int myLocalConst = 22;
            const int newLocalConst = 23;
            var myLocal = greeting + MyProperty;
            bool myChoice = true;
            MyEvent?.Invoke(this, default);
            MyLocalFunction(myParameter);
            goto label;
        label:
            if (myField == (int)MyEnum.MyEnumMember)
            {
                Color.Red.MyExtensionMethod(myLocal);
                Extensions.MyExtensionMethod(Color.Black, myLocal);
                return;
            }
            void MyLocalFunction(T param) { }
        }
    }

    delegate void MyDelegate();
    interface MyInterface<T> { }
    enum MyEnum { MyEnumMember = 5 }
    struct MyStruct { }
    static class Extensions
    {
        public static void MyExtensionMethod(this Color myColor, string myArg) 
        { 
            var myVar = myColor.ToString() + myArg; 
        }
    }
}
#pragma warning restore IDE0051 // Remove unused private members
        public async Task BasicRecordClassification(TestHost testHost)
        {
            await TestAsync(
@"record R
{
    R r;
 
    R() { }
}",
                testHost,
                Record("R"));
        }
        public async Task FunctionPointer(TestHost testHost)
        {
            var code = @"
class C
{
    delegate* unmanaged[Stdcall, SuppressGCTransition] <int, int> x;
}";
 
            await TestAsync(code,
                testHost,
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("delegate"),
                Operators.Asterisk,
                Keyword("unmanaged"),
                Punctuation.OpenBracket,
                Identifier("Stdcall"),
                Punctuation.Comma,
                Identifier("SuppressGCTransition"),
                Punctuation.CloseBracket,
                Punctuation.OpenAngle,
                Keyword("int"),
                Punctuation.Comma,
                Keyword("int"),
                Punctuation.CloseAngle,
                Field("x"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly);
        }