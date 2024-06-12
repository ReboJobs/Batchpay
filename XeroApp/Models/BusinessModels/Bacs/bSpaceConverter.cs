using FileHelpers;

namespace XeroApp.Models.BusinessModels.Bacs
{
    public class bSpaceConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            throw new NotImplementedException();
        }

        public override string FieldToString(object from)
        {
            return base.FieldToString(from).Replace("b" , " ");
        }
    }
}
