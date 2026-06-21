import { useParams } from 'react-router-dom';
import RatCommonForm from '../../components/RatCommonForm';

function CreateEdit() {
    const params = useParams();

    return (
        <RatCommonForm 
            entityName="User" 
            entityId={params.id}
        />
    );
}

export default CreateEdit;