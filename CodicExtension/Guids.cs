using System;

namespace CodicExtension
{
    static class Guids
    {
        public const string CodicExtensionPackageGuid = "CA4CB80A-0216-469E-96AB-80BCA7CAB81F";

        public const string CodicCmdSetGuid = "90EB4DF8-E82B-4712-9CEA-FAC9F711783C";

        public const string OptionsPageGuid = "24AA7F7D-B85E-4076-876D-817DEA5E9300";

        public static readonly Guid guidTranslatorCmdSet = new Guid(CodicCmdSetGuid);

        public const uint GenerationCmdId = 0x100;
    };
}