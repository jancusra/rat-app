export const IsAdminLayout = (pathname: string) => {
    return pathname.startsWith("/admin");
}

export const GetCurrentLanguageId = () => {
    let langId = localStorage.getItem("languageId");

    if (langId === null) {
        langId = "0";
    }

    return Number(langId);
}

export const ChangeStorageItemBoolState = (storageItemKey: string) => {
    if (localStorage.getItem(storageItemKey) === "true") {
        localStorage.setItem(storageItemKey, "false");
    } else {
        localStorage.setItem(storageItemKey, "true");
    }
}