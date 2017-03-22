namespace WEB.Models
{
    public enum AuthorizationType
    {
        None,
        ProtectAll,
        ProtectChanges
    }

    public enum CodeType
    {
        Model,
        DTO,
        DbContext,
        Controller,
        BundleConfig,
        AppRouter,
        ApiResource,
        ListHtml,
        ListTypeScript,
        EditHtml,
        EditTypeScript,
        Enums,
        Global,
        SettingsDTO
    }

    public enum EditPageType
    {
        Normal,
        Exclude,
        ReadOnly,
        EditWhenNew,
        SortField,
        CalculatedField
    }

    public enum EntityType
    {
        Normal,
        User
    }

    public enum FieldType
    {
        Bit,
        Date,
        DateTime,
        Decimal,
        Float,
        Geography,
        Geometry,
        Guid,
        Int,
        Money,
        nVarchar,
        nText,
        SmallDateTime,
        SmallInt,
        Text,
        Time,
        TinyInt,
        Varchar,
        Enum,
        VarBinary
    }

    public enum RelationshipAncestorLimits
    {
        Exclude,
        IncludeRelatedEntity,
        IncludeRelatedParents,
        IncludeAllParents
    }

    public enum SearchType
    {
        None,
        Exact,
        Text
    }

    public static class Extensions
    {
        public static string Label(this AuthorizationType authorizationType)
        {
            switch (authorizationType)
            {
                case AuthorizationType.None:
                    return "None";
                case AuthorizationType.ProtectAll:
                    return "Protect All";
                case AuthorizationType.ProtectChanges:
                    return "Protect Changes";
                default:
                    return null;
            }
        }

        public static string Label(this CodeType codeType)
        {
            switch (codeType)
            {
                case CodeType.Model:
                    return "Model";
                case CodeType.DTO:
                    return "DTO";
                case CodeType.DbContext:
                    return "DbContext";
                case CodeType.Controller:
                    return "Controller";
                case CodeType.BundleConfig:
                    return "BundleConfig";
                case CodeType.AppRouter:
                    return "AppRouter";
                case CodeType.ApiResource:
                    return "ApiResource";
                case CodeType.ListHtml:
                    return "ListHtml";
                case CodeType.ListTypeScript:
                    return "ListTypeScript";
                case CodeType.EditHtml:
                    return "EditHtml";
                case CodeType.EditTypeScript:
                    return "EditTypeScript";
                case CodeType.Enums:
                    return "Enums";
                case CodeType.Global:
                    return "Global";
                case CodeType.SettingsDTO:
                    return "Settings DTO";
                default:
                    return null;
            }
        }

        public static string Label(this EditPageType editPageType)
        {
            switch (editPageType)
            {
                case EditPageType.Normal:
                    return "Normal";
                case EditPageType.Exclude:
                    return "Exclude";
                case EditPageType.ReadOnly:
                    return "Read Only";
                case EditPageType.EditWhenNew:
                    return "Edit When New";
                case EditPageType.SortField:
                    return "Sort Field";
                case EditPageType.CalculatedField:
                    return "Calculated Field";
                default:
                    return null;
            }
        }

        public static string Label(this EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Normal:
                    return "Normal";
                case EntityType.User:
                    return "User";
                default:
                    return null;
            }
        }

        public static string Label(this FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Bit:
                    return "Bit";
                case FieldType.Date:
                    return "Date";
                case FieldType.DateTime:
                    return "DateTime";
                case FieldType.Decimal:
                    return "Decimal";
                case FieldType.Float:
                    return "Float";
                case FieldType.Geography:
                    return "Geography";
                case FieldType.Geometry:
                    return "Geometry";
                case FieldType.Guid:
                    return "Guid";
                case FieldType.Int:
                    return "Int";
                case FieldType.Money:
                    return "Money";
                case FieldType.nVarchar:
                    return "nVarchar";
                case FieldType.nText:
                    return "nText";
                case FieldType.SmallDateTime:
                    return "SmallDateTime";
                case FieldType.SmallInt:
                    return "SmallInt";
                case FieldType.Text:
                    return "Text";
                case FieldType.Time:
                    return "Time";
                case FieldType.TinyInt:
                    return "TinyInt";
                case FieldType.Varchar:
                    return "Varchar";
                case FieldType.Enum:
                    return "Enum";
                case FieldType.VarBinary:
                    return "VarBinary";
                default:
                    return null;
            }
        }

        public static string Label(this RelationshipAncestorLimits relationshipAncestorLimits)
        {
            switch (relationshipAncestorLimits)
            {
                case RelationshipAncestorLimits.Exclude:
                    return "Exclude Entity";
                case RelationshipAncestorLimits.IncludeRelatedEntity:
                    return "Include Related Entity";
                case RelationshipAncestorLimits.IncludeRelatedParents:
                    return "Include Related Parents";
                case RelationshipAncestorLimits.IncludeAllParents:
                    return "Include All Parents";
                default:
                    return null;
            }
        }

        public static string Label(this SearchType searchType)
        {
            switch (searchType)
            {
                case SearchType.None:
                    return "None";
                case SearchType.Exact:
                    return "Exact";
                case SearchType.Text:
                    return "Text";
                default:
                    return null;
            }
        }

    }
}
