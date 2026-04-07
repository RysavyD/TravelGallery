import { useState } from 'react';
import { Platform, StyleSheet, Text, TouchableOpacity } from 'react-native';
import DateTimePicker, { DateTimePickerEvent } from '@react-native-community/datetimepicker';
import { format } from 'date-fns';
import { cs } from 'date-fns/locale';

interface Props {
  value: string; // 'yyyy-MM-dd'
  onChange: (dateStr: string) => void;
}

export default function DatePickerField({ value, onChange }: Props) {
  const [show, setShow] = useState(false);
  const dateValue = value ? new Date(value + 'T00:00:00') : new Date();

  const handleChange = (_event: DateTimePickerEvent, selectedDate?: Date) => {
    if (Platform.OS === 'android') setShow(false);
    if (selectedDate) {
      onChange(format(selectedDate, 'yyyy-MM-dd'));
    }
  };

  return (
    <>
      <TouchableOpacity style={styles.field} onPress={() => setShow(true)}>
        <Text style={styles.fieldText}>
          {value ? format(dateValue, 'd. MMMM yyyy', { locale: cs }) : 'Vyberte datum'}
        </Text>
      </TouchableOpacity>

      {show && (
        <DateTimePicker
          value={dateValue}
          mode="date"
          display={Platform.OS === 'ios' ? 'inline' : 'default'}
          onChange={handleChange}
          locale="cs-CZ"
        />
      )}

      {show && Platform.OS === 'ios' && (
        <TouchableOpacity style={styles.doneBtn} onPress={() => setShow(false)}>
          <Text style={styles.doneBtnText}>Hotovo</Text>
        </TouchableOpacity>
      )}
    </>
  );
}

const styles = StyleSheet.create({
  field: {
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 8,
    padding: 12,
    backgroundColor: '#fafafa',
  },
  fieldText: {
    fontSize: 16,
    color: '#333',
  },
  doneBtn: {
    alignSelf: 'flex-end',
    paddingVertical: 8,
    paddingHorizontal: 16,
    marginTop: 4,
  },
  doneBtnText: {
    fontSize: 16,
    color: '#2196F3',
    fontWeight: '600',
  },
});
