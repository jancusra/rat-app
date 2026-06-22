import { useCallback, useContext, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import RatForm from './RatForm';
import RatCheckbox from './RatCheckbox';
import RatMultiSelect from './RatMultiSelect';
import RatSelect from './RatSelect';
import RatTextField from './RatTextField';
import RatLocales from '../contexts/RatLocales';
import { FormControlState, FormEntry, ValidationResult } from './types';

function RatCommonForm(props: CommonFormProps) {
    const [formData, setFormData] = useState<Array<FormEntry>>([]);
    const [validationData, setValidationData] = useState<ValidationData>({});
    const locales = useContext(RatLocales);
    const navigate = useNavigate();

    const getFormData = useCallback(function () {
        if (props.entityId) {
            axios.post("/entity/getentity/", { id: Number(props.entityId), entityName: props.entityName })
                .then(function (response) {
                    setFormData(response.data);

                    const initialValidation: ValidationData = {};

                    response.data.forEach(function (entry: FormEntry) {
                        initialValidation[entry.name] = { error: false, message: '' };
                    });

                    setValidationData(initialValidation);
                })
                .catch(function (error) {
                    console.error("Failed to load form data", error);
                });
        }
    }, [props.entityId, props.entityName]);

    function updateField(data: FormControlState) {
        const newState = formData.map(obj => {
            if (data.name === obj.name) {
                return { ...obj, value: data.value };
            }
            return obj;
        });

        setFormData(newState);
    }

    function formErrors(errors: Array<ValidationResult>) {
        setValidationData(function (prev) {
            const newState: ValidationData = {};

            Object.keys(prev).forEach(function (name) {
                newState[name] = { error: false, message: '' };
            });

            errors.forEach(function (error) {
                newState[error.fieldName] = { error: true, message: error.message };
            });

            return newState;
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
    }, [getFormData])

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
                switch (formEntry.entryType) {
                    case 'Boolean':
                        return <RatCheckbox
                            key={formEntry.name}
                            name={formEntry.name}
                            label={locales[formEntry.name]}
                            value={formEntry.value as boolean}
                            callback={updateField} />;
                    case 'String':
                        return <RatTextField
                            key={formEntry.name}
                            name={formEntry.name}
                            label={locales[formEntry.name]}
                            value={formEntry.value as string}
                            error={validationData[formEntry.name]?.error}
                            errorMessage={validationData[formEntry.name]?.message}
                            callback={updateField} />;
                    case 'Enum':
                        return <RatSelect
                            key={formEntry.name}
                            name={formEntry.name}
                            label={locales[formEntry.name]}
                            value={formEntry.value as number}
                            selectData={formEntry.selectOptions}
                            callback={updateField} />;
                    case 'MappedMultiSelect':
                        return <RatMultiSelect
                            key={formEntry.name}
                            name={formEntry.name}
                            label={locales[formEntry.name]}
                            value={formEntry.value as Array<number>}
                            selectData={formEntry.selectOptions}
                            callback={updateField} />;
                    default:
                        return null;
                }
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