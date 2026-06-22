import { useContext } from 'react';
import RatUser from '../contexts/RatUser';
import RatLocales from '../contexts/RatLocales';
import RatIcon from '../components/RatIcon';
import { IsAdminLayout, ChangeStorageItemBoolState } from '../Utils';

function RatWebHeader() {
    const user = useContext(RatUser);
    const locales = useContext(RatLocales);

    function changeAdminMenu() {
        ChangeStorageItemBoolState("hiddenAdminMenu");
        window.location.reload();
    }

    function setLanguage(id: number) {
        localStorage.setItem("languageId", id.toString());
        window.location.reload();
    }

    function flagSrc(code: string): string | undefined {
        try {
            return require("./../images/flags/" + code + ".svg");
        } catch {
            return undefined;
        }
    }

    // Activate on Enter/Space for keyboard accessibility
    function onActivateKey(action: () => void) {
        return (e: React.KeyboardEvent) => {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                action();
            }
        };
    }

    return (
        <div className="rat-web-header">
            {IsAdminLayout() ?
                <div className="admin-menu-icon" role="button" tabIndex={0}
                    onClick={changeAdminMenu} onKeyDown={onActivateKey(changeAdminMenu)}>
                    <RatIcon name="menu" />
                </div>
                : null}
            {user.data.email ?
                <div className="logged-user" role="button" tabIndex={0}
                    onClick={() => { location.href = "/" }}
                    onKeyDown={onActivateKey(() => { location.href = "/" })}>
                    Logged as <span className="logged-user-email">{user.data.email}</span>
                </div>
                : null}
            {user.data.isAdmin && !IsAdminLayout() ?
                <div className="admin-link" role="button" tabIndex={0}
                    onClick={() => { location.href = "/admin" }}
                    onKeyDown={onActivateKey(() => { location.href = "/admin" })}>
                    {locales.Administration}
                </div>
                : null}
            {IsAdminLayout() ?
                <div className="admin-link" role="button" tabIndex={0}
                    onClick={() => { location.href = "/" }}
                    onKeyDown={onActivateKey(() => { location.href = "/" })}>
                    {locales.PublicWeb}
                </div>
                : null}
            {Array.isArray(user.data.languages) && user.data.languages.length > 1 ?
                <div className="language-panel">
                    {user.data.languages.map((language) => {
                        let src = flagSrc(language.twoLetterCode);
                        return src ?
                            <img key={language.name} className="language-image"
                                role="button" tabIndex={0}
                                src={src}
                                alt={language.name}
                                onClick={() => setLanguage(language.id)}
                                onKeyDown={onActivateKey(() => setLanguage(language.id))}></img>
                            : null;
                    })}
                </div>
                : null}
        </div>
    );
}

export default RatWebHeader;