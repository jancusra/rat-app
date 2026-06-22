import { useContext } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import RatUser from '../contexts/RatUser';
import RatLocales from '../contexts/RatLocales';
import RatAppContext from '../contexts/RatAppContext';
import RatIcon from '../components/RatIcon';
import { IsAdminLayout } from '../Utils';

function RatWebHeader() {
    const user = useContext(RatUser);
    const locales = useContext(RatLocales);
    const { setLanguage, toggleAdminMenu } = useContext(RatAppContext);
    const location = useLocation();
    const navigate = useNavigate();
    const adminLayout = IsAdminLayout(location.pathname);

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
            {adminLayout ?
                <div className="admin-menu-icon" role="button" tabIndex={0}
                    onClick={toggleAdminMenu} onKeyDown={onActivateKey(toggleAdminMenu)}>
                    <RatIcon name="menu" />
                </div>
                : null}
            {user.data.email ?
                <div className="logged-user" role="button" tabIndex={0}
                    onClick={() => navigate("/")}
                    onKeyDown={onActivateKey(() => navigate("/"))}>
                    Logged as <span className="logged-user-email">{user.data.email}</span>
                </div>
                : null}
            {user.data.isAdmin && !adminLayout ?
                <div className="admin-link" role="button" tabIndex={0}
                    onClick={() => navigate("/admin")}
                    onKeyDown={onActivateKey(() => navigate("/admin"))}>
                    {locales.Administration}
                </div>
                : null}
            {adminLayout ?
                <div className="admin-link" role="button" tabIndex={0}
                    onClick={() => navigate("/")}
                    onKeyDown={onActivateKey(() => navigate("/"))}>
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