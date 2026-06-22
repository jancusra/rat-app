import { useContext } from 'react';
import RatLocales from '../contexts/RatLocales';

function NotFound() {
    const locales = useContext(RatLocales);
    return <div>{locales.PageNotFound}</div>
}

export default NotFound;
