import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { AccessTypeSelect } from '../AccessTypeSelect';

describe('AccessTypeSelect', () => {
  it('should render access type select with all options', () => {
    // Arrange
    const onChange = vi.fn();

    // Act
    render(<AccessTypeSelect value="Public" onChange={onChange} />);

    // Assert
    expect(screen.getByLabelText(/Access Type/i)).toBeInTheDocument();
    expect(screen.getByRole('combobox')).toBeInTheDocument();
  });

  it('should call onChange when selection changes', () => {
    // Arrange
    const onChange = vi.fn();
    render(<AccessTypeSelect value="Public" onChange={onChange} />);

    // Act
    const select = screen.getByRole('combobox');
    fireEvent.change(select, { target: { value: 'Private' } });

    // Assert
    expect(onChange).toHaveBeenCalledWith('Private');
  });

  it('should display selected value', () => {
    // Arrange
    const onChange = vi.fn();

    // Act
    render(<AccessTypeSelect value="Private" onChange={onChange} />);

    // Assert
    const select = screen.getByRole('combobox') as HTMLSelectElement;
    expect(select.value).toBe('Private');
  });

  it('should be required by default', () => {
    // Arrange
    const onChange = vi.fn();

    // Act
    render(<AccessTypeSelect value="Public" onChange={onChange} />);

    // Assert
    const select = screen.getByRole('combobox');
    expect(select).toBeRequired();
  });

  it('should not be required when specified', () => {
    // Arrange
    const onChange = vi.fn();

    // Act
    render(<AccessTypeSelect value="Public" onChange={onChange} required={false} />);

    // Assert
    const select = screen.getByRole('combobox');
    expect(select).not.toBeRequired();
  });

  it('should use custom id when provided', () => {
    // Arrange
    const onChange = vi.fn();
    const customId = 'custom-access-type';

    // Act
    render(<AccessTypeSelect value="Public" onChange={onChange} id={customId} />);

    // Assert
    const select = screen.getByRole('combobox');
    expect(select).toHaveAttribute('id', customId);
  });
});
