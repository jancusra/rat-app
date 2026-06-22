import { useParams } from 'react-router-dom';
import RatCommonForm from './RatCommonForm';

function RatEntityForm(props: EntityFormProps) {
    const params = useParams();

    return (
        <RatCommonForm
            entityName={props.entityName}
            entityId={params.id}
        />
    );
}

export default RatEntityForm;

type EntityFormProps = {
    entityName: string;
}
