import { useParams } from 'react-router-dom';
import RatCommonForm from './RatCommonForm';

type Props = {
    entityName: string;
};

function RatEntityForm({ entityName }: Props) {
    const params = useParams();

    return (
        <RatCommonForm
            entityName={entityName}
            entityId={params.id}
        />
    );
}

export default RatEntityForm;
