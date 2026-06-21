export type Language = {
    id: number;
    name: string;
    isActive: boolean;
    itemOrder: number;
    languageCulture: string;
    twoLetterCode: string;
}

export type LocaleContext = {
    [key: string]: string
}

export type UserData = {
    email?: string;
    isAdmin?: boolean;
    languages?: Array<Language>
}

export type UserContext = {
    data: UserData;
    getUserData: () => void;
}