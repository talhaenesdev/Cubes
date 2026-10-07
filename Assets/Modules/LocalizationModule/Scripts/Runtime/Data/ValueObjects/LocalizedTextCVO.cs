using System;
using UnityEngine;

namespace Modules.LocalizationModule.Data.ValueObjects
{
    // One text in every language. A key no entry lists is shown as it is, so untranslated text still reads.
    [Serializable]
    public class LocalizedTextCVO
    {
        public string Key;
        [TextArea(1, 4)] public string English;
        [TextArea(1, 4)] public string Turkish;
    }
}
