<!--v003-->
# Stargate

***

## Astronaut Career Tracking System (ACTS)

ACTS is used as a tool to maintain a record of all the People that have served as Astronauts. When serving as an Astronaut, your *Job* (Duty) is tracked by your Rank, Title and the Start and End Dates of the Duty.

The People that exist in this system are not all Astronauts. ACTS maintains a master list of People and Duties that are updated from an external service (not controlled by ACTS). The update schedule is determined by the external service.

## Definitions

1. A person's astronaut assignment is the Astronaut Duty.
2. A person's current astronaut information is stored in the Astronaut Detail table.
3. A person's list of astronaut assignments is stored in the Astronaut Duty table.

## Requirements

##### Enhance the Stargate API (Required)

The REST API is expected to do the following:

1. Retrieve a person by name.
2. Retrieve all people.
3. Add/update a person by name.
4. Retrieve Astronaut Duty by name.
5. Add an Astronaut Duty.

##### Implement a user interface: (Required)

The UI is expected to do the following:

1. Successfully run an Angular web application that demonstrates production level quality.
2. Implement call(s) to retrieve an individual's astronaut duties.
3. Display the progress of the process and the results in a visually sophisticated and appealing manner.

## Tasks

Overview
Examine the code, find and resolve any flaws, if any exist. Identify design patterns and follow or change them. Provide fix(es) and be prepared to describe the changes.

1. Generate the database
   * This is your source and storage location
2. Enforce the rules
3. Improve defensive coding
4. Add unit tests
   * identify the most impactful methods requiring tests
   * reach >50% code coverage
5. Implement process logging
   * Log exceptions
   * Log successes
   * Store the logs in the database

## Rules

1. A Person is uniquely identified by their Name.
2. A Person who has not had an astronaut assignment will not have Astronaut records.
3. A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time.
4. A Person's Current Duty will not have a Duty End Date.
5. A Person's Previous Duty End Date is set to the day before the New Astronaut Duty Start Date when a new Astronaut Duty is received for a Person.
6. A Person is classified as 'Retired' when a Duty Title is 'RETIRED'.
7. A Person's Career End Date is one day before the Retired Duty Start Date.