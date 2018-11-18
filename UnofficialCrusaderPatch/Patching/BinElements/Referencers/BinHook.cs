﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnofficialCrusaderPatch
{
    class BinHook : BinRedirect
    {
        public BinHook(string jmpBackLabel, params byte[] jmpBytes)
            : this(jmpBytes.Length + 4, jmpBackLabel, jmpBytes)
        {
        }

        public BinHook(int hookLen, string jmpBackLabel, byte[] jmpBytes)
            : base(true)
        {
            if (hookLen < jmpBytes.Length + 4)
                throw new Exception("Hook length is too short!");

            base.InsertToGroup(0, new BinBytes(jmpBytes));
            int nopsLen = hookLen - (4 + jmpBytes.Length);
            if (nopsLen > 0)
                base.AddToGroup(new BinNops(nopsLen));

            base.Add(new BinBytes(0xE9));
            base.Add(new BinRefTo(jmpBackLabel));
        }

        public override void Add(BinElement input)
        {
            // add in front of jmpBytes, refTo, nops
            EditData.Insert(EditData.Count - 3, input);
        }

        public static Change Change(string ident, ChangeType type, bool checkedDefault, int hookLen, params byte[] code)
        {
            return new Change(ident, type, checkedDefault)
            {
                new DefaultHeader(ident, true)
                {
                    CreateEdit(ident, hookLen, code)
                }
            };
        }

        public static BinaryEdit CreateEdit(string ident, int hookLen, params byte[] code)
        {
            return new BinaryEdit(ident)
            {
                new BinHook(hookLen, ident, new byte[1] { 0xE9 })
                {
                    new BinBytes(code),
                },
                new BinLabel(ident)
            };
        }
    }
}