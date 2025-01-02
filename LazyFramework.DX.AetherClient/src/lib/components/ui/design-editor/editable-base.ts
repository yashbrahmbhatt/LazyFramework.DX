import type { CellContext, ColumnDef, Row } from '@tanstack/table-core';
import type { Writable } from 'svelte/store';

export type ValidationFunction<T> = (
	value: T,
	store: any,
	context: CellContext<any, unknown>
) => string | null;

export type EditableBase = {
	context: CellContext<any, unknown>;
	store: Writable<any>;
	setValue: (store: Writable<any>, value: any, context: CellContext<any, unknown>) => void;
	getValue: (storeValue: any, context: CellContext<any, unknown>) => any;
	validations: ValidationFunction<unknown>[];
};
