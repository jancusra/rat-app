import { useContext, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import RatForm from '../components/RatForm';
import RatCheckbox from '../components/RatCheckbox';
import RatMultiSelect from '../components/RatMultiSelect';
import RatSelect from '../components/RatSelect';
import RatTextField from '../components/RatTextField';
import RatLocales from '../contexts/RatLocales';
import { FormControlState, FormEntry, ValidationResult } from './types';

function RatCommonForm(props: CommonFormProps) {
    const [formData, setFormData] = useState<Array<FormEntry>>([]);
    const [validationData, setValidationData] = useState<ValidationData>({});
    const locales = useContext(RatLocales);
    const navigate = useNavigate();

    function getFormData() {
        if (props.entityId)
        {
            axios.post("/entity/getentity/", { id: parseInt(props.entityId), entityName: props.entityName })
                .then(function (response) {
                    setFormData(response.data);

                    response.data.forEach(function (entry: FormEntry) {
                        validationData[entry.name] = { error: false, message: '' };
                    });
                });
        }
    }

    function updateField(data: FormControlState) {
        const newState = formData.map(obj => {
            if (data.name == obj.name) {
                return { ...obj, value: data.value };
            }
            return obj;
        });
      
        setFormData(newState);
    }

    function formErrors(errors: Array<ValidationResult>) {
        errors.forEach(function (error) {
            setValidationData({
                ...validationData,
                [error.fieldName]: { error: true, message: error.message }
            });
        });
    }

    function formSubmit() {
        if (props.submitUrl) {
            navigate(props.submitUrl);
        } else {
            navigate(-1);
        }
    }

    useEffect(() => {
        getFormData();
    }, [])

    return (
        <RatForm
            class="rat-common-form"
            apiSource="/entity/saveentity"
            entityName={props.entityName}
            buttonContent={locales.Save}
            showCancelButton={true}
            showBackButton={false}
            formData={formData}
            formErrors={formErrors}
            formSubmit={formSubmit}>
            {formData.map((formEntry) => {
                return (
                    {
                        'Boolean': <RatCheckbox
                            key={formEntry.name}
                            name={formEntry.name}
                            label={locales[formEntry.name]}
                            value={formEntry.value as boolean}
                            callback={updateField} />,
                        'String': <RatTextField
                            key={formEntry.name}
                            name={formEntry.name}
                            label={locales[formEntry.name]}
                            value={formEntry.value == null ? '' : formEntry.value as string}
                            error={validationData[formEntry.name].error}
                            errorMessage={validationData[formEntry.name].message}
                            callback={updateField} />,
                        'Enum': <RatSelect
                            key={formEntry.name}
                            name={formEntry.name}
                            label={locales[formEntry.name]}
                            value={formEntry.value as string}
                            selectData={formEntry.selectOptions}
                            callback={updateField} />,
                        'MappedMultiSelect': <RatMultiSelect
                            key={formEntry.name}
                            name={formEntry.name}
                            label={locales[formEntry.name]}
                            value={formEntry.value as Array<number>}
                            selectData={formEntry.selectOptions}
                            callback={updateField} />
                    } [formEntry.entryType]
                );
            })}
        </RatForm>
    );
}

export default RatCommonForm;

type CommonFormProps = {
    entityId?: string;
    entityName: string;
    submitUrl?: string;
}

type ValidationEntry = {
    error: boolean;
    message: string;
}

type ValidationData = {
    [key: string]: ValidationEntry;
}