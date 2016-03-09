using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyORM.VSExtension
{
    class Guids
    {
        public const string guidEasyORM_XLinq_VSExtensionPkgString = "faaa9b76-ec59-4f07-bc18-9f1387b400a9";
        public const string guidEasyORM_XLinq_VSExtensionCmdSetString = "c2133bc9-7ad2-4784-b4c9-e45b55ef5aac";
        public const string guidEditorFactoryString = "2CFC3E40-2A51-4010-BCE4-1FF548A9661E";

        public static readonly Guid guidEasyORM_XLinq_VSExtensionCmdSet = new Guid(guidEasyORM_XLinq_VSExtensionCmdSetString);
        public static readonly Guid guidEditorFactory = new Guid(guidEditorFactoryString);
    }
}
