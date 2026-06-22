import React from 'react';
import { AppContext } from '../types';

const defaultValue: AppContext = {
    languageId: 0,
    setLanguage: () => void 0,
    hiddenAdminMenu: false,
    toggleAdminMenu: () => void 0,
};

const RatAppContext = React.createContext<AppContext>(defaultValue);

export default RatAppContext;
