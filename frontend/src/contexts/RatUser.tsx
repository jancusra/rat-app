import { createContext } from 'react';
import { UserContext } from '../types';

const defaultValue: UserContext = {
    data: {},
    getUserData: () => Promise.resolve(),
    clearUserData: () => void 0
};

const RatUser = createContext<UserContext>(defaultValue);

export default RatUser;