export type SelectOptions = {
    [key: number]: string
}

export type FormControlState = {
    name: string;
    value: number | string | boolean | Array<number> | Array<string>;
}

export type RatFormData = {
    [key: string]: number | string | boolean | Array<number> | Array<string>
}

// Shared shape for form fields and grid columns
export type EntityField = {
    entryType: string;
    excluded: boolean;
    hidden: boolean;
    name: string;
    order: number;
    selectOptions: SelectOptions;
}

export type FormEntry = EntityField & {
    value: number | string | boolean | Array<number> | Array<string>;
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