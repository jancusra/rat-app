import { useCallback, useContext, useEffect, useState } from 'react';
import axios from 'axios';
import RatBoolIcon from './RatBoolIcon';
import RatLocales from '../contexts/RatLocales';
import { FormEntry } from './types';

function renderValue(entry: FormEntry) {
    if (typeof entry.value === "boolean") {
        return <RatBoolIcon value={entry.value} />;
    }

    const hasOptions = entry.selectOptions != null && Object.keys(entry.selectOptions).length > 0;

    if (hasOptions && Array.isArray(entry.value)) {
        return entry.value
            .map((id) => entry.selectOptions[id as number])
            .filter(Boolean)
            .join(", ");
    }

    if (hasOptions && typeof entry.value === "number") {
        return entry.selectOptions[entry.value];
    }

    return entry.value;
}

function RatCommonDetail(props: CommonDetailProps) {
    const [detailData, setDetailData] = useState<Array<FormEntry>>([]);
    const locales = useContext(RatLocales);

    const getDetailData = useCallback(function () {
        if (props.entityId) {
            axios.post("/entity/getEntity", { id: Number(props.entityId), entityName: props.entityName })
                .then(function (response) {
                    setDetailData(response.data);
                })
                .catch(function (error) {
                    console.error("Failed to load detail data", error);
                });
        }
    }, [props.entityId, props.entityName]);

    useEffect(() => {
        getDetailData();
    }, [getDetailData])

    return (
        <table className='table-detail'>
            <tbody>
                {detailData.map((detailEntry) => {
                    if (detailEntry.name !== "Id")
                        return (
                            <tr key={detailEntry.name}>
                                <td className="detail-name">{locales[detailEntry.name]}:</td>
                                <td>{renderValue(detailEntry)}</td>
                            </tr>
                        );
                    return null
                })}
            </tbody>
        </table>
    );
}

export default RatCommonDetail;

type CommonDetailProps = {
    entityId?: string;
    entityName: string;
}