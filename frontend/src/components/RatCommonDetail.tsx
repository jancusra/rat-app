import { useCallback, useContext, useEffect, useState } from 'react';
import axios from 'axios';
import RatLocales from '../contexts/RatLocales';
import { FormEntry } from './types';

function RatCommonDetail(props: CommonDetailProps) {
    const [detailData, setDetailData] = useState<Array<FormEntry>>([]);
    const locales = useContext(RatLocales);

    const getDetailData = useCallback(function () {
        if (props.entityId)
        {
            axios.post("/entity/getentity/", { id: parseInt(props.entityId), entityName: props.entityName })
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
                    if (detailEntry.name != "Id")
                        return (
                            <tr key={detailEntry.name}>
                                <td className="detail-name">{locales[detailEntry.name]}:</td>
                                {detailEntry.selectOptions != null && Object.keys(detailEntry.selectOptions).length > 0 
                                    && typeof detailEntry.value == "number"
                                    ? <td>{detailEntry.selectOptions[detailEntry.value]}</td>
                                    : <td>{detailEntry.value}</td>}
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