export type SelectOptions = {
    [key: number]: string
}

// Value a form field or grid cell can hold
export type FieldValue = number | string | boolean | Array<number> | Array<string>;

export type FormControlState = {
    name: string;
    value: FieldValue;
}

export type RatFormData = {
    [key: string]: FieldValue
}

// Entry kinds shared by form fields and grid columns
export type EntryType =
    | "String"
    | "Boolean"
    | "DateTime"
    | "Enum"
    | "EnumIcon"
    | "MappedMultiSelect"
    | "ShowDetailButton"
    | "EditInViewButton"
    | "DeleteButton";

// Shared shape for form fields and grid columns
export type EntityField = {
    entryType: EntryType;
    excluded: boolean;
    hidden: boolean;
    name: string;
    order: number;
    selectOptions: SelectOptions;
}

export type FormEntry = EntityField & {
    value: FieldValue;
}

export type GridColumn = EntityField & {
    flex: number;
    width: number;
}

export type ValidationResult = {
    fieldName: string;
    message: string;
}

export type TreeMenuItem = {
    id: number;
    title: string;
    url: string;
    icon: string;
    childMenuItems: Array<TreeMenuItem>;
}