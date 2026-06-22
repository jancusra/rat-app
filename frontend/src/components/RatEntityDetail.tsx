import { useParams } from 'react-router-dom';
import RatCommonDetail from './RatCommonDetail';

function RatEntityDetail(props: EntityDetailProps) {
    const params = useParams();

    return (
        <RatCommonDetail
            entityName={props.entityName}
            entityId={params.id}
        />
    );
}

export default RatEntityDetail;

type EntityDetailProps = {
    entityName: string;
}
