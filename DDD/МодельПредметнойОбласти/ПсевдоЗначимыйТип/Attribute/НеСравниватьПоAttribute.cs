namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти;

public abstract partial class ПсевдоЗначимыйТип {
    /// <summary>
    ///     Attribute, предназначеный для обозначения полей и свойств <see cref="ПсевдоЗначимыйТип" />, которые не должны
    ///     участвовать в сравнении их экземпляров между собой
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class НеСравниватьПоAttribute : Attribute {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public string Причина { get; set; } = "Служебное";
    }
}