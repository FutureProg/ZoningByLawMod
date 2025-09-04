using System.Collections.Generic;

namespace ZoningByLaw.UISystems
{
    public class FieldDataOption
    {
        public string? image;
        public string label;
        public object value;
    }

    public abstract class FieldDataBase
    {
        public string id;
        public string label;
        public List<FieldDataOption>? options;
    }

    public class CheckboxFieldData : FieldDataBase
    {
        public string fieldType = "checkbox";
        public object[] value; // array of selected values
    }

    public class RadioFieldData : FieldDataBase
    {
        public string fieldType = "radio";
        public object value; // single selected value
    }

    public class SelectFieldData : FieldDataBase
    {
        public string fieldType = "select";
        public object value; // single selected value
    }

    public class TextFieldData : FieldDataBase
    {
        public string fieldType = "text";
        public string value;
        public string? validationRegex;
    }

    public class NumberFieldData : FieldDataBase
    {
        public string fieldType = "number";
        public bool? slider;
        public double value;
        public double? min;
        public double? max;
        public double? step;
    }

    public class RangeFieldData : FieldDataBase
    {
        public string fieldType = "range";
        public double[] value; // [min, max]
        public double? min;
        public double? max;
        public double? step;
    }
}