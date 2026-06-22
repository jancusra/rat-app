import { useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import Button from '@mui/material/Button';
import RatGrid from './RatGrid';
import RatLocales from '../contexts/RatLocales';

type Props = {
    entityName: string;
    createNew?: boolean;
};

function RatEntityList({ entityName, createNew }: Props) {
    const navigate = useNavigate();
    const locales = useContext(RatLocales);

    return (
        <>
            {createNew &&
                <Button variant="contained"
                    color="success"
                    onClick={() => navigate("./0")}>{locales.CreateNew}</Button>}
            <RatGrid entityName={entityName} />
        </>
    );
}

export default RatEntityList;
