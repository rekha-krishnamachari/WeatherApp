# AI Notes

## 1. AI tool(s) used

I used GitHub Copilot as the AI assistant during this project.

## 2. Prompts that helped the most

1. "I’m seeing an issue where the app does not show a message when the weather API fails. How would you identify the root cause and fix it in Angular?"
2. "How would you add an error state to an Angular component so users can see a meaningful message instead of a blank or broken screen?"
3. "Implement DateFileReader that accepts and tests formats like MM/dd/yyyy, M/d/yyyy, MMMM d, yyyy, MMM-dd-yyyy, and 'natural' inputs. Ensure strict invalid-date detection (e.g., April 31 invalid), timezone handling for the API query, and unit tests for leap years and ambiguous formats."

## 3. Concrete example where the AI suggestion was wrong or not ideal

I rejected the first AI suggestion because it exposed the raw HttpErrorResponse object directly to the UI. That is not user-friendly and it can be unhelpful or even blank. I validated the actual error structure, extracted a readable message from the nested error payload, and then displayed only that clean message to the user while still logging the full error for debugging.

This was the correct fix because the service was already making the API call successfully; the problem was that the component was only logging the error instead of storing and showing a meaningful message in the UI.

## 4. Parts of the solution where I chose to write code myself rather than rely on AI

I wrote the core fix myself in the component and template because the issue was about real UI state management, not just generating a generic snippet. The service already called the API correctly; the missing behavior was that the component did not store or display an error message.

I also wrote the final UI logic manually for:

- adding an `errorMessage` property to the component
- setting it in the `error` callback
- displaying it conditionally in the template
- resetting it when the data request succeeds
- styling the message so the user can clearly see the failure state

This was important because the fix had to match the actual Angular data flow and the app’s user experience requirements, rather than using a generic AI-generated pattern that might not fit the project exactly.
