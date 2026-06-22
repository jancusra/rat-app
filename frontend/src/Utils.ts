export const STORAGE_LANGUAGE_ID = "languageId";
export const STORAGE_HIDDEN_ADMIN_MENU = "hiddenAdminMenu";

export const isAdminLayout = (pathname: string) => {
    return pathname.startsWith("/admin");
}

export const getCurrentLanguageId = () => {
    return Number(localStorage.getItem(STORAGE_LANGUAGE_ID) ?? "0");
}

export const changeStorageItemBoolState = (storageItemKey: string) => {
    if (localStorage.getItem(storageItemKey) === "true") {
        localStorage.setItem(storageItemKey, "false");
    } else {
        localStorage.setItem(storageItemKey, "true");
    }
}
