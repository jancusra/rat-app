import { createContext } from 'react';
import { LocaleContext } from '../types';

const RatLocales = createContext<LocaleContext>({});

export default RatLocales;