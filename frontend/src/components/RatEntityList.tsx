import { useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import Button from '@mui/material/Button';
import RatGrid from './RatGrid';
import RatLocales from '../contexts/RatLocales';

function RatEntityList(props: EntityListProps) {
    const navigate = useNavigate();
    const locales = useContext(RatLocales);

    return (
        <>
            {props.createNew &&
                <Button variant="contained"
                    color="success"
                    onClick={() => navigate("./0")}>{locales.CreateNew}</Button>}
            <RatGrid entityName={props.entityName} />
        </>
    );
}

export default RatEntityList;

type EntityListProps = {
    entityName: string;
    createNew?: boolean;
}
