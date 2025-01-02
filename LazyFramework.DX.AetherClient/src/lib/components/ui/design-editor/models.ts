import type { ColumnDef } from '@tanstack/table-core';
import { v4 as uuid } from 'uuid';

export type RunnableScript = {
	script: string;
	inputs: any;
	executable: string;
};

export class Template {
	id: string = uuid();
	name: string = 'Blank Process';
	description: string = 'This is a blank process module';
	root: string = '';
	configurations: any[] = [];
	dependencies: string[] = [];
	setup: RunnableScript = {
		script: '',
		inputs: {},
		executable: ''
	};
	variations: RunnableScript[] = [];
}

export class Robot {
	id: string = uuid();
	name: string = 'Blank Process';
	description: string = 'This is a blank process module';
	moduleType: ModuleType.Process = ModuleType.Process;
	attended: boolean = false;
	persistent: boolean = false;
	libraries: string[] = [];
	queues: QueueReference[] = [];
	tasks: string[] = [];
}

export class Library {
	id: string = uuid();
	name: string = 'Blank Library';
	description: string = 'This is a blank library module';
	referenced: boolean = false;
	application: string = '';
	moduleType: ModuleType.Library = ModuleType.Library;
	processes: string[] = [];
}

export enum ApplicationType {
	Database = 'database',
	WebAPI = 'webapi',
	Terminal = 'terminal',
	GUI = 'gui',
	IntegrationService = 'integration_service'
}

export enum ModuleType {
	Process = 'process',
	Library = 'library'
}

export class QueueReference {
	queueId: string = '';
	populate: boolean = false;
	consume: boolean = false;
	reference: boolean = false;
}

export class Queue {
	id: string = uuid();
	name: string = 'New Queue';
	folder: string = '';
	description: string = 'This is a new queue';
	inputSchema: any = {};
	outputSchema: any = {};
}

export class ActionCenterTask {
	id: string = uuid();
	name: string = 'New Task';
	folder: string = '';
	catalog: string = 'New Catalog';
	type: ActionCenterTaskType = ActionCenterTaskType.Form;
	description: string = 'This is a new task';
	inputSchema: any = {};
	outputSchema: any = {};
}

export class TargetApplication {
	id: string = uuid();
	name: string = 'New Target Application';
	description: string = 'This is a new target application';
	applicationType: ApplicationType = ApplicationType.IntegrationService;
}

export enum ActionCenterTaskType {
	App = 'app',
	Form = 'form',
	DataLabelling = 'data_labelling',
	DocumentClassification = 'document_classification',
	DocumentValidation = 'document_validation',
	External = 'external'
}

export enum TriggerType {
	Manual = 'manual',
	Schedule = 'schedule',
	Queue = 'queue'
}

export class Trigger {
	id: string = uuid();
	name: string = 'New Trigger';
	description: string = 'This is a new trigger';
	triggerType: TriggerType = TriggerType.Manual;
	queueId: string = '';
	cron: string = '';
	robotId: string = '';
}
export interface SolutionData {
	[x: string]: any[];
	libraries: Library[];
	robots: Robot[];
	queues: Queue[];
	tasks: ActionCenterTask[];
	applications: TargetApplication[];
	triggers: Trigger[];
}
export interface SolutionColumns {
	[x: string]: ColumnDef<any>[];
	robots: ColumnDef<Robot>[];
	libraries: ColumnDef<Library>[];
	queues: ColumnDef<Queue>[];
	tasks: ColumnDef<ActionCenterTask>[];
	applications: ColumnDef<TargetApplication>[];
	triggers: ColumnDef<Trigger>[];
}
export function createSampleData(): SolutionData {
	const ids = {
		library: {
			email: uuid(),
			application: uuid()
		},
		robots: {
			dispatcher: uuid(),
			performer: uuid()
		},
		queues: {
			performer: uuid()
		},
		tasks: {
			validate: uuid()
		},
		applications: {
			application: uuid(),
			email: uuid()
		},
		triggers: {
			dispatcher: uuid(),
			performer: uuid()
		}
	};
	return {
		libraries: [
			{
				id: ids.library.email,
				name: 'UiPath.Office365.Activities',
				description: 'This is a library module',
				moduleType: ModuleType.Library,
				referenced: true,
				application: ids.applications.email,
				processes: [ids.robots.dispatcher, ids.robots.performer]
			},
			{
				id: ids.library.application,
				name: 'Application 1',
				description: 'This is a library module',
				moduleType: ModuleType.Library,
				referenced: false,
				application: ids.applications.application,
				processes: [ids.robots.dispatcher, ids.robots.performer]
			}
		],
		robots: [
			{
				id: ids.robots.dispatcher,
				name: 'Dispatcher',
				description: 'This is the dispatcher module',
				moduleType: ModuleType.Process,
				attended: false,
				libraries: [ids.library.application, ids.library.email],
				persistent: false,
				queues: [
					{
						queueId: ids.queues.performer,
						populate: true,
						consume: false,
						reference: false
					}
				],
				tasks: []
			},
			{
				id: ids.robots.performer,
				name: 'Performer',
				description: 'This is the performer module',
				moduleType: ModuleType.Process,
				attended: false,
				libraries: [ids.library.application, ids.library.email],
				persistent: false,
				queues: [
					{
						queueId: ids.queues.performer,
						populate: false,
						consume: true,
						reference: false
					}
				],
				tasks: [ids.tasks.validate]
			}
		],
		queues: [
			{
				id: ids.queues.performer,
				name: '001_Performer',
				folder: '',
				description: 'This is the performer queue',
				inputSchema: {
					data_field_1: 'string',
					data_field_2: 0
				},
				outputSchema: {}
			}
		],
		tasks: [
			{
				id: ids.tasks.validate,
				name: 'Validation Task',
				folder: '',
				catalog: 'Catalog 1',
				type: ActionCenterTaskType.App,
				description: 'This is a task',
				inputSchema: {},
				outputSchema: {}
			}
		],
		applications: [
			{
				id: ids.applications.application,
				name: 'Application 1',
				description: 'No APIs available, need to use GUI automation.',
				applicationType: ApplicationType.GUI
			},
			{
				id: ids.applications.email,
				name: 'Office 365 - Outlook',
				description: 'Emailing will be handled through Office 365',
				applicationType: ApplicationType.IntegrationService
			}
		],
		triggers: [
			{
				id: ids.triggers.dispatcher,
				name: 'Start',
				description: 'Scheduled start of dispatcher',
				triggerType: TriggerType.Schedule,
				cron: '0 0 0 * * *',
				queueId: '',
				robotId: ids.robots.dispatcher
			},
			{
				id: ids.triggers.performer,
				name: 'Trigger Robots',
				description: 'Dynamic start of performers',
				triggerType: TriggerType.Queue,
				queueId: ids.queues.performer,
				robotId: ids.robots.performer,
				cron: ''
			}
		]
	};
}
