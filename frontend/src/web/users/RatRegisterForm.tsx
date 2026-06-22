import { useContext, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import RatForm from '../../components/RatForm';
import RatTextField from '../../components/RatTextField';
import RatUser from '../../contexts/RatUser';
import RatLocales from '../../contexts/RatLocales';
import { FormControlState, RatFormData } from '../../components/types';

function RatRegisterForm() {
    const [formData, setState] = useState<RatFormData>({});
    const user = useContext(RatUser);
    const locales = useContext(RatLocales);
    const navigate = useNavigate();

    function updateField(data: FormControlState) {
        setState({
            ...formData,
            [data.name]: data.value
        });
    }

    function formSubmit() {
        user.getUserData();
        navigate("/");
    }

    return (
        <RatForm
            class="rat-register-box"
            apiSource="/user/register"
            buttonContent={locales.Register}
            showBackButton={true}
            formData={formData}
            formSubmit={formSubmit}>
            <RatTextField
                name="email"
                label={locales.Email}
                value={String(formData.email ?? '')}
                callback={updateField} />
            <RatTextField
                name="password"
                type="password"
                label={locales.Password}
                value={String(formData.password ?? '')}
                callback={updateField} />
            <RatTextField
                name="passwordVerify"
                type="password"
                label={locales.PasswordVerify}
                value={String(formData.passwordVerify ?? '')}
                callback={updateField} />
        </RatForm>
    );
}

export default RatRegisterForm;