using System.Collections.Generic;
using Trejak.ZoningByLaw.BuildingBlocks;

namespace ZoningByLaw.UISystems
{
    public class FieldDataOption<T>
    {
        public string? image;
        public string label;
        public T value;
    }

    public abstract class FieldDataBase
    {
        public string id;
        public string label;
        public List<FieldDataOption<int>>? options;
        public List<FieldDataOption<ByLawPropertyOperator>> operatorOptions;
        public ByLawPropertyOperator selectedOperator;
        public object value;
    }

    public class CheckboxFieldData : FieldDataBase
    {
        public string fieldType = "checkbox";
        public new int[] value; // array of selected values
    }

    public class RadioFieldData : FieldDataBase
    {
        public string fieldType = "radio";
        public new int value; // single selected value
    }

    public class SelectFieldData : FieldDataBase
    {
        public string fieldType = "select";
        public new object value; // single selected value
    }

    public class TextFieldData : FieldDataBase
    {
        public string fieldType = "text";
        public new string value;
        public string? validationRegex;
    }

    public class NumberFieldData : FieldDataBase
    {
        public string fieldType = "number";
        public bool? slider;
        public new double value;
        public double? min;
        public double? max;
        public double? step;
    }

    public class RangeFieldData : FieldDataBase
    {
        public string fieldType = "range";
        public new double[] value; // [min, max]
        public double? min;
        public double? max;
        public double? step;
    }
}