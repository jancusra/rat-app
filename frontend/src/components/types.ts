export type SelectOptions = {
    [key: number]: string
}

export type FormControlState = {
    name: string;
    value: number | string | boolean | Array<number> | Array<string>;
}

export type FormData = {
    [key: string]: number | string | boolean | Array<number> | Array<string>
}

export type FormEntry = {
    entryType: string;
    excluded: boolean;
    hidden: boolean;
    name: string;
    order: number;
    selectOptions: SelectOptions;
    value: number | string | boolean | Array<number> | Array<string>;
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