import { useParams } from 'react-router-dom';
import RatCommonDetail from './RatCommonDetail';

type Props = {
    entityName: string;
};

function RatEntityDetail({ entityName }: Props) {
    const params = useParams();

    return (
        <RatCommonDetail
            entityName={entityName}
            entityId={params.id}
        />
    );
}

export default RatEntityDetail;
