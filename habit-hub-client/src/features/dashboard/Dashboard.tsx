export const Dashboard = () => {
 const name:string  = localStorage.getItem('userName') ?? 'User';
  return(
  <div>
    <h1 className="text-3xl font-bold text-gray-900 mb-4">Dashboard</h1>
    <p className="text-gray-600">{name}, welcome to HabitHub!</p>
  </div>
   );
  }