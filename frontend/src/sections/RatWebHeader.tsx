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

    return (
        <div className="rat-web-header">
            {IsAdminLayout() ?
                <div className="admin-menu-icon" onClick={changeAdminMenu}>
                    <RatIcon name="menu" />
                </div>
                : null}
            {user.data.email ?
                <div className="logged-user" onClick={() => {location.href = "/"}}>
                    Logged as <span className="logged-user-email">{user.data.email}</span>
                </div>
            : null}
            {user.data.isAdmin && !IsAdminLayout() ?
                <div className="admin-link" onClick={() => {location.href = "/admin"}}>
                    {locales.Administration}
                </div>
            : null}
            {IsAdminLayout() ?
                <div className="admin-link" onClick={() => {location.href = "/"}}>
                    {locales.PublicWeb}
                </div>
            : null}
            {Array.isArray(user.data.languages) && user.data.languages.length > 1 ?
                <div className="language-panel">
                    {user.data.languages.map((language) =>
                        <img key={language.name} className="language-image" 
                            src={require("./../images/flags/" + language.twoLetterCode + ".svg")}
                            alt={language.name}
                            onClick={() => setLanguage(language.id)}></img>
                    )}
                </div>
            : null}
        </div>
    );
}

export default RatWebHeader;