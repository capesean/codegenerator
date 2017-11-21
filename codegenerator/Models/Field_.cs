using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public partial class Field
    {
        [NotMapped]
        public string NewVariable
        {
            get
            {
                if (FieldType == FieldType.Guid) return "newGuid";
                if (FieldType == FieldType.Int) return "newInt";
                if (FieldType == FieldType.SmallInt) return "newInt";
                if (FieldType == FieldType.TinyInt) return "newInt";
                if (FieldType == FieldType.Date) return "newDate";
                if (CustomType == CustomType.String) return "newString"; // changed from string.Empty to newString as string.Empty appears to be server side and this should be client side code?
                throw new NotImplementedException("NewVariable for Type: " + FieldType);
            }
        }

        [NotMapped]
        public string EmptyValue
        {
            get
            {
                if (FieldType == FieldType.Guid) return "Guid.Empty";
                if (FieldType == FieldType.Int) return "0";
                if (FieldType == FieldType.SmallInt) return "0";
                if (FieldType == FieldType.TinyInt) return "0";
                if (FieldType == FieldType.Date) return "DateTime.MinValue";
                if (CustomType == CustomType.String) return "string.Empty";
                throw new NotImplementedException("EmptyValue for Type: " + FieldType);
            }
        }

        [NotMapped]
        public CustomType CustomType
        {
            get
            {
                switch (FieldType)
                {
                    case FieldType.Enum:
                        return CustomType.Enum;
                    case FieldType.Bit:
                        return CustomType.Boolean;
                    case FieldType.Date:
                    case FieldType.DateTime:
                    case FieldType.SmallDateTime:
                        return CustomType.Date;
                    case FieldType.Guid:
                        return CustomType.Guid;
                    case FieldType.Int:
                    case FieldType.TinyInt:
                    case FieldType.SmallInt:
                    case FieldType.Decimal:
                        return CustomType.Number;
                    case FieldType.nVarchar:
                    case FieldType.nText:
                    case FieldType.Text:
                    case FieldType.Varchar:
                        return CustomType.String;
                    case FieldType.VarBinary:
                        return CustomType.Binary;
                }
                throw new NotImplementedException("CustomType: " + FieldType.ToString());
            }
        }

        [NotMapped]
        public string ControllerConstraintType
        {
            get
            {
                switch (FieldType)
                {
                    case FieldType.Bit:
                        return "bool";
                    case FieldType.Date:
                    case FieldType.DateTime:
                    case FieldType.SmallDateTime:
                        return "DateTime";
                    case FieldType.Decimal:
                        return "decimal";
                    case FieldType.Guid:
                        return "Guid";
                    case FieldType.Int:
                    case FieldType.TinyInt:
                    case FieldType.SmallInt:
                        return "int";
                    case FieldType.nVarchar:
                    case FieldType.nText:
                    case FieldType.Text:
                    case FieldType.Varchar:
                        return "string";
                }
                throw new NotImplementedException("ControllerConstraintType: " + FieldType.ToString());
            }
        }

        public static string GetNetType(FieldType fieldType, bool isNullable, Lookup lookup)
        {
            switch (fieldType)
            {
                case FieldType.Enum:
                    // this is used when using an enum as a search field, needs to get the type as int
                    //if(Lookup == null) return "int" + (IsNullable ? "?" : string.Empty);
                    return lookup.Name + (isNullable ? "?" : string.Empty);
                case FieldType.Bit:
                    return "bool" + (isNullable ? "?" : string.Empty);
                case FieldType.Date:
                case FieldType.DateTime:
                case FieldType.SmallDateTime:
                    return "DateTime" + (isNullable ? "?" : string.Empty);
                case FieldType.Guid:
                    return "Guid" + (isNullable ? "?" : string.Empty);
                case FieldType.Int:
                    return "int" + (isNullable ? "?" : string.Empty);
                case FieldType.TinyInt:
                    return "byte" + (isNullable ? "?" : string.Empty);
                case FieldType.SmallInt:
                    return "short" + (isNullable ? "?" : string.Empty);
                case FieldType.Decimal:
                    return "decimal" + (isNullable ? "?" : string.Empty);
                case FieldType.nVarchar:
                case FieldType.nText:
                case FieldType.Text:
                case FieldType.Varchar:
                    return "string";
                case FieldType.VarBinary:
                    return "byte[]" + (isNullable ? "?" : string.Empty);
            }
            throw new NotImplementedException("NetType: " + fieldType.ToString());
        }

        [NotMapped]
        public string NetType
        {
            get
            {
                return GetNetType(FieldType, IsNullable, Lookup);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum CustomType
    {
        Enum, Boolean, Date, Guid, Number, String, Binary
    }

}
