import type { RequestHandler } from './$types';
import fs from 'fs';
export const GET: RequestHandler = async () => {
	var raw = fs.readFileSync(
		'C:\\Users\\eyash\\OneDrive\\Documents\\UiPath\\RoboticEnterpriseFramework4\\project.json', 'utf8'
	);

	var res = JSON.stringify({
		text: raw
	}) as BodyInit;
	return new Response(res);
};
