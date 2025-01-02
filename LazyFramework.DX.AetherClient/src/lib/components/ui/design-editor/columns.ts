import type { CellContext, ColumnDef, Row, RowData } from '@tanstack/table-core';
import { renderComponent } from '../data-table';
import EditableTextCell from './editable-text-cell.svelte';
import { writable, type Writable } from 'svelte/store';
import {
	ActionCenterTaskType,
	ApplicationType,
	createSampleData,
	Robot,
	Queue,
	QueueReference,
	TriggerType,
	type SolutionColumns,
	type SolutionData
} from './models';
import EditableToggleCell from './editable-toggle-cell.svelte';
import ValidatedEditableTextareaCell from './editable-textarea-cell.svelte';
import type { ValidationFunction } from './editable-base';
import EditableSelectCell from './editable-select-cell.svelte';
import EditableTableSheetCell from './editable-table-sheet-cell.svelte';

export function createBooleanColumnDef(
	key: string,
	header: string,
	tooltip: string,
	store: Writable<any>,
	table: string,
	trueText?: string,
	falseText?: string,
	validations: ValidationFunction<unknown>[] = [],
	getValue: (storeValue: any, context: CellContext<any, unknown>) => any = (s, c) =>
		s[table][c.row.index][c.column.id],
	setValue: (store: Writable<any>, value: any, context: CellContext<any, unknown>) => void = (
		s,
		v,
		c
	) =>
		s.update((prev) => {
			prev[table][c.row.index][c.column.id] = v;
			return prev;
		})
) {
	return {
		id: key,
		accessorKey: key,
		header: header,
		cell: (context: CellContext<any, unknown>) => {
			return renderComponent(EditableToggleCell, {
				context,
				getValue,
				store,
				setValue,
				trueText,
				falseText,
				validations
			});
		},
		meta: {
			tooltip
		}
	};
}

export function createValidatedEditableTextColumnDef<T>(
	key: string,
	header: string,
	tooltip: string,
	store: Writable<any>,
	table: string,
	validations: ValidationFunction<unknown>[] = [
		(v, s, c) => validateNotEmpty(v as string, c, s, header)
	],
	getValue: (storeValue: any, context: CellContext<any, unknown>) => any = (s, c) =>
		s[table][c.row.index][c.column.id],
	setValue: (store: Writable<any>, value: any, context: CellContext<any, unknown>) => void = (
		s,
		v,
		c
	) =>
		s.update((prev) => {
			prev[table][c.row.index][c.column.id] = v;
			return prev;
		})
): ColumnDef<T> {
	return {
		id: key,
		accessorKey: key,
		header: header,
		meta: {
			tooltip
		},
		cell: (context: CellContext<any, unknown>) => {
			return renderComponent(EditableTextCell, {
				context,
				getValue,
				store,
				setValue,
				validations
			});
		}
	};
}

export function createValidatedEditableTextAreaColumnDef<T>(
	key: string,
	header: string,
	tooltip: string,
	store: Writable<any>,
	table: string,
	validations: ValidationFunction<unknown>[] = [],
	getValue: (storeValue: any, context: CellContext<any, unknown>) => any = (s, c) =>
		s[table][c.row.index][c.column.id],
	setValue: (store: Writable<any>, value: any, context: CellContext<any, unknown>) => void = (
		s,
		v,
		c
	) =>
		s.update((prev) => {
			prev[table][c.row.index][c.column.id] = v;
			return prev;
		})
): ColumnDef<T> {
	return {
		id: key,
		accessorKey: key,
		header: header,
		meta: {
			tooltip
		},
		cell: (context: CellContext<any, unknown>) => {
			return renderComponent(ValidatedEditableTextareaCell, {
				context,
				store,
				setValue,
				getValue,
				validations
			});
		}
	};
}

export function createSelectColumnDef(
	key: string,
	header: string,
	tooltip: string,
	store: Writable<any>,
	table: string,
	getOptions: (store: any, context: CellContext<any, unknown>) => { value: any; label: string }[],
	validations: ValidationFunction<unknown>[] = [],
	getValue: (storeValue: any, context: CellContext<any, unknown>) => any = (s, c) =>
		s[table][c.row.index][c.column.id],
	setValue: (store: Writable<any>, value: any, context: CellContext<any, unknown>) => void = (
		s,
		v,
		c
	) =>
		s.update((prev) => {
			prev[table][c.row.index][c.column.id] = v;
			return prev;
		})
) {
	return {
		id: key,
		accessorKey: key,
		header: header,
		meta: {
			tooltip
		},
		cell: (ctx: CellContext<any, unknown>) => {
			return renderComponent(EditableSelectCell, {
				context: ctx,
				store,
				getOptions,
				getValue,
				setValue,
				validations
			});
		}
	};
}

export function createTableSheetColumnDef<T>(
	key: string,
	header: string,
	tooltip: string,
	store: Writable<any>,
	table: string,
	getColumns: (
		originalStore: Writable<any>,
		newStore: Writable<any>,
		originalValue: any,
		newValue: any,
		context: CellContext<any, unknown>
	) => ColumnDef<any>[],
	getTriggerText: (value: any, context: CellContext<any, unknown>) => string,
	validations: ValidationFunction<unknown>[] = [],
	getValue: (storeValue: any, context: CellContext<any, unknown>) => any = (s, c) =>
		s[table][c.row.index][c.column.id],
	setValue: (store: Writable<any>, value: any, context: CellContext<any, unknown>) => void = (
		s,
		v,
		c
	) =>
		s.update((prev) => {
			prev[table][c.row.index][c.column.id] = v;
			return prev;
		})
): ColumnDef<T> {
	return {
		id: key,
		accessorKey: key,
		header: header,
		meta: {
			tooltip
		},
		cell: (context: CellContext<any, unknown>) => {
			return renderComponent(EditableTableSheetCell, {
				context,
				store,
				setValue,
				getValue,
				validations,
				getColumns,
				getTriggerText
			});
		}
	};
}

export interface ColumnMeta<TData extends RowData, TValue> {
	tooltip: string;
}

export const data = createSampleData();

function validateNotEmpty(
	value: string,
	context: CellContext<any, unknown>,
	store: Writable<any>,
	key: string
) {
	if (!value) {
		return `${key} cannot be empty`;
	}
	return null;
}

function validateValidJson(
	value: string,
	context: CellContext<any, unknown>,
	store: Writable<any>,
	key: string
) {
	try {
		JSON.parse(value);
	} catch (e) {
		return `${key} must be valid JSON`;
	}
	return null;
}

export function createDesignEditorColumns(store: Writable<SolutionData>): SolutionColumns {
	return {
		robots: [
			createValidatedEditableTextColumnDef('name', 'Name', 'The robot name', store, 'robots'),
			createValidatedEditableTextColumnDef(
				'description',
				'Description',
				'The robot description',
				store,
				'robots'
			),
			createBooleanColumnDef(
				'attended',
				'Attended',
				"Whether the robot is designed to be run on a user's machine",
				store,
				'robots',
				'Attended',
				'Unattended'
			),
			createBooleanColumnDef(
				'persistent',
				'Persistent',
				"Whether the robot will need to support persistence in it's workflows.",
				store,
				'robots',
				'Persistent',
				'Non-Persistent'
			),
			createTableSheetColumnDef(
				'queues',
				'Queues',
				'The queues that the robot is associated with',
				store,
				'robots',
				(ogStore, newStore, ogValue, newValue, context) => {
					return [
						createValidatedEditableTextColumnDef(
							'name',
							'Name',
							'The name of the queue',
							newStore,
							'',
							[(v, s, c) => validateNotEmpty(v as string, c, s, 'Name')],
							(s, c) => s[c.row.index].name,
							(s, v, c) =>
								s.update((prev) => {
									prev[c.row.index].name = v;
									return prev;
								})
						),
						createBooleanColumnDef(
							'consume',
							'Consume',
							'Whether the queue is consumed by the robot',
							newStore,
							'',
							'Yes',
							'No',
							[],
							(s, c) => s[c.row.index].consume,
							(s, v, c) =>
								s.update((prev) => {
									prev[c.row.index].consume = v;
									return prev;
								})
						),
						createBooleanColumnDef(
							'populate',
							'Populate',
							'Whether the queue is populated by the robot',
							newStore,
							'',
							'Yes',
							'No',
							[],
							(s, c) => s[c.row.index].populate,
							(s, v, c) =>
								s.update((prev) => {
									prev[c.row.index].populate = v;
									return prev;
								})
						),
						createBooleanColumnDef(
							'reference',
							'Reference',
							'Whether the queue is referenced by the robot',
							newStore,
							'',
							'Yes',
							'No',
							[],
							(s, c) => s[c.row.index].reference,
							(s, v, c) =>
								s.update((prev) => {
									prev[c.row.index].reference = v;
									return prev;
								})
						)
					];
				},
				(value, context) => {
					if (value.length === 0) {
						return 'Select queues';
					} else {
						const consumed = value.filter((v: any) => v.consume);
						const populated = value.filter((v: any) => v.populate);
						const referenced = value.filter((v: any) => v.reference);
						let text = '';
						if (consumed.length > 0) {
							text += `Consumed: ${consumed.length}, `;
						}
						if (populated.length > 0) {
							text += `Populated: ${populated.length}, `;
						}
						if (referenced.length > 0) {
							text += `Referenced: ${referenced.length}`;
						}
						return text.replace(/,\s*$/, '');
					}
				},
				[],
				(s, c) => {
					return s.queues.map((queue: Queue) => {
						const robotReference = s.robots[c.row.index].queues.find(
							(q: QueueReference) => q.queueId === queue.id
						);
						return {
							id: robotReference.queueId,
							name: queue.name,
							consume: robotReference.consume,
							populate: robotReference.populate,
							reference: robotReference.reference
						};
					});
				},
				(s, v, c) => {
					s.update((prev) => {
						const value = v.reduce((acc: QueueReference[], queue: any) => {
							if (!queue.consume && !queue.populate && !queue.reference) return acc;
							acc.push({
								queueId: queue.id,
								consume: queue.consume,
								populate: queue.populate,
								reference: queue.reference
							});
							return acc;
						}, []);
						prev.robots[c.row.index].queues = value;
						return prev;
					});
				}
			),
			createTableSheetColumnDef(
				'tasks',
				'Tasks',
				'The tasks that the robot is associated with',
				store,
				'robots',
				(ogStore, newStore, ogValue, newValue, context) => {
					return [
						createValidatedEditableTextColumnDef(
							'name',
							'Name',
							'The name of the task',
							newStore,
							'',
							[(v, s, c) => validateNotEmpty(v as string, c, s, 'Name')],
							(s, c) => s[c.row.index].name,
							(s, v, c) =>
								s.update((prev) => {
									prev[c.row.index].name = v;
									return prev;
								})
						),
						createBooleanColumnDef(
							'use',
							'Used',
							'Whether the task is used by the robot',
							newStore,
							'',
							'Yes',
							'No',
							[],
							(s, c) => s[c.row.index].used,
							(s, v, c) =>
								s.update((prev) => {
									prev[c.row.index].used = v;
									return prev;
								})
						)
					];
				},
				(value, context) => {
					if (value.length === 0) {
						return 'Select tasks';
					} else {
						const used = value.filter((v: any) => v.used);
						return `Used: ${used.length}`;
					}
				},
				[],
				(s, c) => {
					return s.tasks.map((task: Robot) => {
						const robotReference = s.robots[c.row.index].tasks.includes(task.id);
						return {
							id: task.id,
							name: task.name,
							used: robotReference
						};
					});
				},
				(s, v, c) => {
					s.update((prev) => {
						const value = v.reduce((acc: string[], task: any) => {
							if (!task.used) return acc;
							acc.push(task.id);
							return acc;
						}, []);
						prev.robots[c.row.index].tasks = value;
						return prev;
					});
				}
			),
			createTableSheetColumnDef(
				'libraries',
				'Libraries',
				'The libraries that the robot is associated with',
				store,
				'robots',
				(ogStore, newStore, ogValue, newValue, context) => {
					return [
						createValidatedEditableTextColumnDef(
							'name',
							'Name',
							'The name of the library',
							newStore,
							'',
							[(v, s, c) => validateNotEmpty(v as string, c, s, 'Name')],
							(s, c) => s[c.row.index].name,
							(s, v, c) =>
								s.update((prev) => {
									prev[c.row.index].name = v;
									return prev;
								})
						),
						createBooleanColumnDef(
							'reference',
							'Referenced',
							'Whether the library is referenced by the robot',
							newStore,
							'',
							'Yes',
							'No',
							[],
							(s, c) => s[c.row.index].reference,
							(s, v, c) =>
								s.update((prev) => {
									prev[c.row.index].reference = v;
									return prev;
								})
						)
					];
				},
				(value, context) => {
					if (value.length === 0) {
						return 'Select libraries';
					} else {
						const referenced = value.filter((v: any) => v.reference);
						return `Referenced: ${referenced.length}`;
					}
				},
				[],
				(s, c) => {
					return s.libraries.map((library: Robot) => {
						const robotReference = s.robots[c.row.index].libraries.includes(library.id);
						return {
							id: library.id,
							name: library.name,
							reference: robotReference
						};
					});
				},
				(s, v, c) => {
					s.update((prev) => {
						const value = v.reduce((acc: string[], library: any) => {
							if (!library.reference) return acc;
							acc.push(library.id);
							return acc;
						}, []);
						prev.robots[c.row.index].libraries = value;
						console.log('prev', prev);
						return prev;
					});
				}
			)
		],
		queues: [
			createValidatedEditableTextColumnDef('name', 'Name', 'The queue name', store, 'queues'),
			createValidatedEditableTextColumnDef(
				'description',
				'Description',
				'The queue description',
				store,
				'queues'
			),
			createValidatedEditableTextColumnDef(
				'folder',
				'Folder',
				'The folder the queue is in',
				store,
				'queues',
				[]
			),
			createValidatedEditableTextAreaColumnDef(
				'inputSchema',
				'Input',
				'The schema for the input data',
				store,
				'queues',
				[(v, s, c) => validateValidJson(v as string, c, s, 'Input')],
				(s, c) => {
					return JSON.stringify(s.queues[c.row.index].inputSchema, null, 2);
				}
			),
			createValidatedEditableTextAreaColumnDef(
				'outputSchema',
				'Output',
				'The schema for the output data',
				store,
				'queues',
				[(v, s, c) => validateValidJson(v as string, c, s, 'Output')],
				(s, c) => {
					return JSON.stringify(s.queues[c.row.index].outputSchema, null, 2);
				}
			)
		],
		libraries: [
			createValidatedEditableTextColumnDef(
				'name',
				'Name',
				"The library's name",
				store,
				'libraries'
			),
			createValidatedEditableTextColumnDef(
				'description',
				'Description',
				"The library's description",
				store,
				'libraries'
			),
			createBooleanColumnDef(
				'referenced',
				'Referenced',
				'Whether the library will be maintained by this solution or is just a reference to an existing library',
				store,
				'libraries',
				'Referenced',
				'Developed'
			),
			createSelectColumnDef(
				'application',
				'Application',
				'The application that the library is associated with',
				store,
				'libraries',
				(store, context) => {
					return [
						...store.applications.map((application: Robot) => {
							return {
								value: application.id,
								label: application.name
							};
						}),
						{
							value: '',
							label: 'No Application'
						}
					];
				}
			)
		],
		tasks: [
			createValidatedEditableTextColumnDef('name', 'Name', 'The task name', store, 'tasks'),
			createValidatedEditableTextColumnDef(
				'description',
				'Description',
				'The task description',
				store,
				'tasks'
			),
			createValidatedEditableTextColumnDef(
				'folder',
				'Folder',
				'The folder the task is in',
				store,
				'tasks',
				[]
			),
			createValidatedEditableTextColumnDef(
				'catalog',
				'Catalog',
				'The catalog the task is in',
				store,
				'tasks'
			),
			createSelectColumnDef(
				'type',
				'Type',
				'The type of task',
				store,
				'tasks',
				(store, context) => [
					...Object.keys(ActionCenterTaskType)
						.filter((key) => isNaN(Number(key))) // Exclude numeric keys
						.map((key) => ({
							value: ActionCenterTaskType[key as keyof typeof ActionCenterTaskType], // Enum value
							label: key // Enum key
						})),
					{
						value: '',
						label: 'No Task Type'
					}
				]
			),

			createValidatedEditableTextAreaColumnDef(
				'inputSchema',
				'Input',
				'The schema for the input data',
				store,
				'tasks',
				[(v, s, c) => validateValidJson(v as string, c, s, 'Input')],
				(s, c) => {
					return JSON.stringify(s.tasks[c.row.index].inputSchema, null, 2);
				}
			),
			createValidatedEditableTextAreaColumnDef(
				'outputSchema',
				'Output',
				'The schema for the output data',
				store,
				'tasks',
				[(v, s, c) => validateValidJson(v as string, c, s, 'Output')],
				(s, c) => {
					return JSON.stringify(s.tasks[c.row.index].outputSchema, null, 2);
				}
			)
		],
		applications: [
			createValidatedEditableTextColumnDef(
				'name',
				'Name',
				'The application name',
				store,
				'applications'
			),
			createValidatedEditableTextColumnDef(
				'description',
				'Description',
				'The application description',
				store,
				'applications'
			),
			createSelectColumnDef(
				'applicationType',
				'Integration Type',
				'The type of interaction between the robot and the application',
				store,
				'applications',
				(store, context) => [
					...Object.keys(ApplicationType)
						.filter((key) => isNaN(Number(key))) // Exclude numeric keys
						.map((key) => ({
							value: ApplicationType[key as keyof typeof ApplicationType], // Enum value
							label: key // Enum key
						})),
					{
						value: '',
						label: 'No Application Type'
					}
				]
			)
		],
		triggers: [
			createValidatedEditableTextColumnDef('name', 'Name', 'The trigger name', store, 'triggers'),
			createValidatedEditableTextColumnDef(
				'description',
				'Description',
				'The trigger description',
				store,
				'triggers'
			),
			createSelectColumnDef(
				'triggerType',
				'Type',
				'The type of the trigger; how the trigger works',
				store,
				'triggers',
				(store, context) => [
					...Object.keys(TriggerType)
						.filter((key) => isNaN(Number(key))) // Exclude numeric keys
						.map((key) => ({
							value: TriggerType[key as keyof typeof TriggerType], // Enum value
							label: key // Enum key
						}))
				]
			),
			createValidatedEditableTextColumnDef(
				'cron',
				'Cron',
				'The cron expression for the trigger',
				store,
				'triggers',
				[]
			),
			createSelectColumnDef(
				'queueId',
				'Queue',
				'The queue that the trigger is associated with',
				store,
				'triggers',
				(store, context) => {
					return [
						...store.queues.map((queue: Queue) => {
							return {
								value: queue.id,
								label: queue.name
							};
						}),
						{
							value: '',
							label: 'No Queue'
						}
					];
				}
			),
			createSelectColumnDef(
				'robotId',
				'Robot',
				'The robot that will be triggered',
				store,
				'triggers',
				(store, context) => {
					return store.robots.map((robot: Robot) => {
						return {
							value: robot.id,
							label: robot.name
						};
					});
				}
			)
		]
	};
}
