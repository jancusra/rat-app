import { useContext, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import RatForm from '../../components/RatForm';
import RatTextField from '../../components/RatTextField';
import RatUser from '../../contexts/RatUser';
import RatLocales from '../../contexts/RatLocales';
import { FormControlState, RatFormData } from '../../components/types';

type Props = {
    mode: 'login' | 'register';
};

function RatAuthForm({ mode }: Props) {
    const [formData, setState] = useState<RatFormData>({});
    const user = useContext(RatUser);
    const locales = useContext(RatLocales);
    const navigate = useNavigate();

    const isRegister = mode === 'register';

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
            class={isRegister ? "rat-register-box" : "rat-login-box"}
            apiSource={isRegister ? "/user/register" : "/auth/authenticate"}
            buttonContent={isRegister ? locales.Register : locales.Login}
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
            {isRegister &&
                <RatTextField
                    name="passwordVerify"
                    type="password"
                    label={locales.PasswordVerify}
                    value={String(formData.passwordVerify ?? '')}
                    callback={updateField} />}
        </RatForm>
    );
}

export default RatAuthForm;
