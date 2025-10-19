import { Spin } from 'antd';

interface LoadingSpinnerProps {
  size?: 'small' | 'default' | 'large';
  tip?: string;
}

const LoadingSpinner = ({ size = 'default', tip }: LoadingSpinnerProps) => {
  return (
    <div style={{ textAlign: 'center', padding: '50px' }}>
      <Spin size={size} tip={tip} />
    </div>
  );
};

export default LoadingSpinner;
