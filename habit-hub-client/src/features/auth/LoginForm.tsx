import { useForm } from 'react-hook-form';
import api from '../../api/axiosInstance';
import { useNavigate } from 'react-router-dom';


interface LoginFormInputs {
  email: string;
  password: string;
}

export const LoginForm: React.FC = () => {

  const {register, handleSubmit, formState: { errors }} = useForm<LoginFormInputs>();
  const navigate = useNavigate();

  const onSubmit = async (data: LoginFormInputs) =>{
    try {
      const response = await api.post('/auth/login', data);
      localStorage.setItem('sessionId', response.data.sessionId);
      navigate('/dashboard');
    } catch (error) {
      console.error("Login error", error);
    }
     
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <label>Email</label>
        <input 
          {...register("email", { 
            required: "Email is required", 
            pattern: { value: /^\S+@\S+$/i, message: "Incorrect email format" } 
          })} 
          className={errors.email ? 'border-red-500' : ''}
        />
        {errors.email && <p className="text-red-500">{errors.email.message}</p>}
      </div>

      <div>
        <label>Password</label>
        <input 
          type="password"
          {...register("password", { required: "Enter the password" })} 
        />
        {errors.password && <p className="text-red-500">{errors.password.message}</p>}
      </div>

      <button type="submit">Login</button>
    </form>
  );
}