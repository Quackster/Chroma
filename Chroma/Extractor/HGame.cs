using Flazzy;
using Flazzy.ABC;
using Flazzy.ABC.AVM2;
using Flazzy.ABC.AVM2.Instructions;
using Flazzy.IO;
using Flazzy.Records;
using Flazzy.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Extractor
{
    class HGame : ShockwaveFlash
    {
        private readonly Dictionary<DoABCTag, ABCFile> _abcFileTags;
        public List<ABCFile> ABCFiles { get; }
        public string Location { get; set; }

        public HGame(string path)
            : this(File.OpenRead(path))
        {
            Location = path;
        }
        public HGame(byte[] data)
            : this(new MemoryStream(data))
        { }
        public HGame(Stream input)
            : this(input, false)
        { }
        public HGame(Stream input, bool leaveOpen)
            : this(new FlashReader(input, leaveOpen))
        { }
        protected HGame(FlashReader input)
            : base(input)
        {
            _abcFileTags = new Dictionary<DoABCTag, ABCFile>();

            ABCFiles = new List<ABCFile>();
        }

        protected override void WriteTag(TagItem tag, FlashWriter output)
        {
            if (tag.Kind == TagKind.DoABC)
            {
                var doABCTag = (DoABCTag)tag;
                doABCTag.ABCData = _abcFileTags[doABCTag].ToArray();
            }
            base.WriteTag(tag, output);
        }
        protected override TagItem ReadTag(HeaderRecord header, FlashReader input)
        {
            TagItem tag = base.ReadTag(header, input);
            if (tag.Kind == TagKind.DoABC)
            {
                var doABCTag = (DoABCTag)tag;
                var abcFile = new ABCFile(doABCTag.ABCData);

                _abcFileTags[doABCTag] = abcFile;
                ABCFiles.Add(abcFile);
            }
            return tag;
        }

        public void InjectMessageLogger()
        {
            ABCFile abc = ABCFiles.Last();
            ASInstance decoderInstance = null;
            foreach (ASInstance instance in abc.Instances)
            {
                if (instance.IsInterface) continue;
                if (instance.Traits.Count != 12) continue;
                if (instance.Constructor.Parameters.Count != 2) continue;
                if (instance.Constructor.Parameters[0].Type.Name != "int") continue;
                if (instance.Constructor.Parameters[1].Type.Name != "ByteArray") continue;

                decoderInstance = instance;
                break;
            }

            var methods = decoderInstance.GetMethods();
            var i = 0;
            foreach (var method in methods)
            {
                ASCode methodCode = method.Body.ParseCode();

                switch (i)
                {
                    case 0:
                        // set header
                        methodCode.InsertRange(methodCode.Count - 1, new ASInstruction[]
                        {
                            new ConvertIIns(),
                            new SetLocal1Ins(),
                            new GetLexIns(abc, abc.Pool.GetMultinameIndex("ExternalInterface")),
                            new PushStringIns(abc, "FlashExternalInterface.logIncomingHeader"),
                            new GetLocal1Ins(),
                            new CallPropVoidIns(abc, abc.Pool.GetMultinameIndex("call"), 2),
                            new GetLocal1Ins()
                        });
                        break;
                    case 1:
                        // readString
                        methodCode.InsertRange(methodCode.Count - 1, new ASInstruction[]
                        {
                            new ConvertSIns(),
                            new SetLocal1Ins(),
                            new GetLexIns(abc, abc.Pool.GetMultinameIndex("ExternalInterface")),
                            new PushStringIns(abc, "FlashExternalInterface.logIncoming"),
                            new PushStringIns(abc, "string"),
                            new GetLocal1Ins(),
                            new CallPropVoidIns(abc, abc.Pool.GetMultinameIndex("call"), 3),
                            new GetLocal1Ins()
                        });
                        break;
                    case 2:
                        // readInteger
                        methodCode.InsertRange(methodCode.Count - 1, new ASInstruction[]
                        {
                            new ConvertIIns(),
                            new SetLocal1Ins(),
                            new GetLexIns(abc, abc.Pool.GetMultinameIndex("ExternalInterface")),
                            new PushStringIns(abc, "FlashExternalInterface.logIncoming"),
                            new PushStringIns(abc, "integer"),
                            new GetLocal1Ins(),
                            new CallPropVoidIns(abc, abc.Pool.GetMultinameIndex("call"), 3),
                            new GetLocal1Ins()
                        });
                        break;
                    case 3:
                        // readBoolean
                        methodCode.InsertRange(methodCode.Count - 1, new ASInstruction[]
                        {
                            new ConvertBIns(),
                            new SetLocal1Ins(),
                            new GetLexIns(abc, abc.Pool.GetMultinameIndex("ExternalInterface")),
                            new PushStringIns(abc, "FlashExternalInterface.logIncoming"),
                            new PushStringIns(abc, "boolean"),
                            new GetLocal1Ins(),
                            new CallPropVoidIns(abc, abc.Pool.GetMultinameIndex("call"), 3),
                            new GetLocal1Ins()
                        });
                        break;
                    case 4:
                        // readShort
                        methodCode.InsertRange(methodCode.Count - 1, new ASInstruction[]
                        {
                            new ConvertIIns(),
                            new SetLocal1Ins(),
                            new GetLexIns(abc, abc.Pool.GetMultinameIndex("ExternalInterface")),
                            new PushStringIns(abc, "FlashExternalInterface.logIncoming"),
                            new PushStringIns(abc, "short"),
                            new GetLocal1Ins(),
                            new CallPropVoidIns(abc, abc.Pool.GetMultinameIndex("call"), 3),
                            new GetLocal1Ins()
                        });
                        break;
                    case 5:
                        // readByte
                        methodCode.InsertRange(methodCode.Count - 1, new ASInstruction[]
                        {
                            new ConvertIIns(),
                            new SetLocal1Ins(),
                            new GetLexIns(abc, abc.Pool.GetMultinameIndex("ExternalInterface")),
                            new PushStringIns(abc, "FlashExternalInterface.logIncoming"),
                            new PushStringIns(abc, "byte"),
                            new GetLocal1Ins(),
                            new CallPropVoidIns(abc, abc.Pool.GetMultinameIndex("call"), 3),
                            new GetLocal1Ins()
                        });
                        break;
                    case 6:
                        // readFloat
                        methodCode.InsertRange(methodCode.Count - 1, new ASInstruction[]
                        {
                            new ConvertDIns(),
                            new SetLocal1Ins(),
                            new GetLexIns(abc, abc.Pool.GetMultinameIndex("ExternalInterface")),
                            new PushStringIns(abc, "FlashExternalInterface.logIncoming"),
                            new PushStringIns(abc, "float"),
                            new GetLocal1Ins(),
                            new CallPropVoidIns(abc, abc.Pool.GetMultinameIndex("call"), 3),
                            new GetLocal1Ins()
                        });
                        break;
                }
                i++;

                method.Body.Code = methodCode.ToArray();
                method.Body.LocalCount += 2;
                method.Body.MaxStack += 3;
                Console.WriteLine("Patched function " + i);
            }
        }
    }
}
